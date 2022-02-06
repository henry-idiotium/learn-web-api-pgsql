using System.Collections.Generic;
using System.Threading.Tasks;
using Sieve.Models;
using WepA.Models.Dtos.Common;
using WepA.Models.Dtos.Token;
using WepA.Models.Entities;

namespace WepA.Interfaces.Services
{
	public interface IUserService
	{
		Task AddRefreshTokenAsync(ApplicationUser user, RefreshToken refreshToken);
		Task CreateAsync(CreateUserRequest model);
		Task DeleteAsync(string userId);
		Task<UserDetailsResponse> GetByEmailAsync(string email);
		Task<UserDetailsResponse> GetByIdAsync(string userId);
		ObjectListResponse GetList(SieveModel search);
		Task MockCreateAsync(List<CreateUserRequest> models);
		Task RevokeRefreshToken(string token);
		Task<AuthenticateResponse> RotateTokensAsync(TokenRotateRequest model);
		Task UpdateAsync(ApplicationUser user);
		bool ValidateExistence(ApplicationUser user);
	}
}