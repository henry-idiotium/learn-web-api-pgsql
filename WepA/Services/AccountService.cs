using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WepA.Helpers;
using WepA.Helpers.ResponseMessages;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Token;
using WepA.Models.Entities;

namespace WepA.Services
{
	public class AccountService : IAccountService
	{
		private readonly IEmailService _emailService;
		private readonly IJwtService _jwtService;
		private readonly ILogger _logger;
		private readonly IMapper _mapper;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IUserService _userService;

		public AccountService(
			IEmailService emailService,
			IJwtService jwtService,
			ILogger<AccountService> logger,
			IMapper mapper,
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IUserService userService)
		{
			_emailService = emailService;
			_jwtService = jwtService;
			_logger = logger;
			_mapper = mapper;
			_signInManager = signInManager;
			_userManager = userManager;
			_userService = userService;
		}

		public async Task<AuthenticateResponse> LoginAsync(LoginRequest account)
		{
			var result = await _signInManager.PasswordSignInAsync(account.Email, account.Password,
				isPersistent: false,
				lockoutOnFailure: false);
			if (!result.Succeeded)
			{
				throw new HttpStatusException(HttpStatusCode.Unauthorized,
											  ErrorResponseMessages.FailedLogin);
			}
			var user = await _userManager.FindByEmailAsync(account.Email);
			if (!await _userManager.IsEmailConfirmedAsync(user))
			{
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.EmailNotVerify);
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Name, user.UserName),
			};
			var accessToken = _jwtService.GenerateAccessToken(claims);
			var refreshToken = _jwtService.GenerateRefreshToken(user.Id);
			await _userService.AddRefreshTokenAsync(user, refreshToken);

			user.Id = EncryptHelpers.EncodeBase64Url(user.Id);
			return new AuthenticateResponse(accessToken, refreshToken.Token, user);
		}

		public async Task RegisterAsync(RegisterRequest model)
		{
			if (model.Password != model.ConfirmPassword)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			model.UserName ??= model.Email;
			var user = _mapper.Map<ApplicationUser>(model);

			if (_userService.ValidateExistence(user) || model.Password != model.ConfirmPassword)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.UserAlreadyExists);

			var createUser = await _userManager.CreateAsync(user, model.Password);
			if (!createUser.Succeeded)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			var addRole = await _userManager.AddToRoleAsync(user, "customer");
			if (!addRole.Succeeded)
			{
				_logger.LogError($"Failed to add role to user {user.Email}", addRole.Errors);
				throw new HttpStatusException(HttpStatusCode.InternalServerError,
											  ErrorResponseMessages.UnexpectedError);
			}

			var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var encodedEmailToken = EncryptHelpers.EncodeBase64Url(confirmEmailToken);
			await _emailService.SendConfirmEmailAsync(user, encodedEmailToken);

			var refreshToken = _jwtService.GenerateRefreshToken(user.Id);
			await _userService.AddRefreshTokenAsync(user, refreshToken);
		}

		public async Task VerifyEmailAsync(string userId, string token)
		{
			if (userId == null || token == null)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (!result.Succeeded)
				throw new HttpStatusException(HttpStatusCode.InternalServerError,
											  ErrorResponseMessages.UnexpectedError);
		}
	}
}