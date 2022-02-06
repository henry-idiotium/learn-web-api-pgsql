using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;
using WepA.Helpers;
using WepA.Helpers.ResponseMessages;
using WepA.Interfaces.Repositories;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Common;
using WepA.Models.Dtos.Token;
using WepA.Models.Entities;

namespace WepA.Services
{
	public class UserService : IUserService
	{
		private readonly IEmailService _emailService;
		private readonly IJwtService _jwtService;
		private readonly ILogger _logger;
		private readonly IMapper _mapper;
		private readonly SieveOptions _sieveOptions;
		private readonly SieveProcessor _sieveProcessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IUserRepository _userRepository;

		public UserService(IEmailService emailService, IJwtService jwtService,
			ILogger<UserService> logger,
			IMapper mapper,
			IOptions<SieveOptions> sieveOptions,
			SieveProcessor sieveProcessor,
			UserManager<ApplicationUser> userManager,
			IUserRepository userRepository)
		{
			_emailService = emailService;
			_jwtService = jwtService;
			_logger = logger;
			_mapper = mapper;
			_sieveOptions = sieveOptions.Value;
			_sieveProcessor = sieveProcessor;
			_userManager = userManager;
			_userRepository = userRepository;
		}

		public async Task AddRefreshTokenAsync(ApplicationUser user, RefreshToken refreshToken)
		{
			if (refreshToken == null || !refreshToken.IsActive)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidTokens);

			await _userRepository.AddRefreshTokenAsync(user, refreshToken);
		}

		public async Task CreateAsync(CreateUserRequest model)
		{
			if (model.Password != model.ConfirmPassword)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.PasswordNotMatch);

			model.UserName ??= model.Email;
			var user = _mapper.Map<ApplicationUser>(model);

			if (ValidateExistence(user) || model.Password != model.ConfirmPassword)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.UserAlreadyExists);

			var createUser = await _userManager.CreateAsync(user, model.Password);
			if (!createUser.Succeeded)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			var addRoles = await _userManager.AddToRolesAsync(user, model.Roles);
			if (!addRoles.Succeeded)
			{
				_logger.LogError($"Failed to add role to user {user.Email}.", addRoles.Errors);
				throw new HttpStatusException(HttpStatusCode.InternalServerError,
											  ErrorResponseMessages.UnexpectedError);
			}

			var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var code = EncryptHelpers.EncodeBase64Url(confirmToken);

			await _emailService.SendConfirmEmailAsync(user, code);
		}

		public Task DeleteAsync(string userId) => throw new NotImplementedException();

		public async Task<UserDetailsResponse> GetByEmailAsync(string email) =>
			_mapper.Map<UserDetailsResponse>(await _userManager.FindByEmailAsync(email));

		public async Task<UserDetailsResponse> GetByIdAsync(string userId) =>
			_mapper.Map<UserDetailsResponse>(await _userManager.FindByIdAsync(userId));

		public ObjectListResponse GetList(SieveModel model)
		{
			if (model?.Page < 0 || model?.PageSize < 1)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			var users = _userRepository.GetUsers();
			var sortedUsers = _sieveProcessor
				.Apply(model, _mapper.Map<IEnumerable<UserDetailsResponse>>(users)
									 .AsQueryable());

			var pageSize = model.PageSize ?? _sieveOptions.DefaultPageSize;

			return new ObjectListResponse(sortedUsers, sortedUsers.Count(),
				currentPage: model.Page ?? 1,
				totalPages: (int)Math.Ceiling((float)users.Count() / pageSize));
		}

		public async Task MockCreateAsync(List<CreateUserRequest> models)
		{
			foreach (var m in models)
			{
				if (m.Password != m.ConfirmPassword)
					throw new HttpStatusException(HttpStatusCode.BadRequest,
												  ErrorResponseMessages.PasswordNotMatch);

				m.UserName ??= m.Email;
				var user = _mapper.Map<ApplicationUser>(m);

				if (ValidateExistence(user) || m.Password != m.ConfirmPassword)
					throw new HttpStatusException(HttpStatusCode.BadRequest,
												  ErrorResponseMessages.UserAlreadyExists);

				var createUser = await _userManager.CreateAsync(user, m.Password);
				if (!createUser.Succeeded)
					throw new HttpStatusException(HttpStatusCode.BadRequest,
												  ErrorResponseMessages.InvalidRequest);

				var addRoles = await _userManager.AddToRolesAsync(user, m.Roles);
				if (!addRoles.Succeeded)
				{
					_logger.LogError($"Failed to add role to user {user.Email}.", addRoles.Errors);
					throw new HttpStatusException(HttpStatusCode.InternalServerError,
												  ErrorResponseMessages.UnexpectedError);
				}

				var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				await _userManager.ConfirmEmailAsync(user, confirmToken);
			}
		}

		public async Task RevokeRefreshToken(string token)
		{
			var refreshToken = _userRepository.GetRefreshToken(token);
			if (!refreshToken.IsActive)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidTokens);

			await _userRepository.RevokeRefreshTokenAsync(refreshToken, "Revoked without replacement");
		}

		public async Task<AuthenticateResponse> RotateTokensAsync(TokenRotateRequest model)
		{
			var user = _userRepository.GetByRefreshToken(model.RefreshToken);
			var refreshToken = _userRepository.GetRefreshToken(model.RefreshToken);
			if (refreshToken == null)
				throw new HttpStatusException(HttpStatusCode.Unauthorized,
											  ErrorResponseMessages.InvalidRequest);

			if (refreshToken.IsRevoked)
				// revoke all descendant tokens in case this token has been compromised
				await _userRepository.RevokeRefreshTokenDescendantsAsync(refreshToken, user,
					reason: $"Attempted reuse of revoked ancestor token: {model.RefreshToken}");

			if (!refreshToken.IsActive)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidTokens);

			// rotate token
			var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id);
			await _userRepository.RevokeRefreshTokenAsync(
				token: refreshToken,
				reason: "Replaced by new token",
				replacedByToken: newRefreshToken.Token);
			await _userRepository.RemoveOutdatedRefreshTokensAsync(user);

			// Get principal from expired token
			var principal = _jwtService.GetClaimsPrincipal(model.AccessToken);
			if (principal == null)
				throw new HttpStatusException(HttpStatusCode.Unauthorized,
											  ErrorResponseMessages.InvalidRequest);

			var accessToken = _jwtService.GenerateAccessToken(principal.Claims);
			return new AuthenticateResponse(accessToken, newRefreshToken.Token, user);
		}

		public Task UpdateAsync(ApplicationUser user) => throw new NotImplementedException();

		public bool ValidateExistence(ApplicationUser user)
		{
			if (user == null)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			var userExists = _userRepository.ValidateExistence(user);
			return userExists;
		}
	}
}