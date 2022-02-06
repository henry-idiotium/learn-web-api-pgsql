using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WepA.Interfaces.Repositories;
using WepA.Models.Entities;

namespace WepA.Data.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly WepADbContext _context;

		public UserRepository(WepADbContext context) => _context = context;

		public async Task<bool> AddRefreshTokenAsync(ApplicationUser user, RefreshToken refreshToken)
		{
			user.RefreshTokens.Add(refreshToken);
			_context.Update(user);
			var created = await _context.SaveChangesAsync();
			return created > 0;
		}

		public ApplicationUser GetByRefreshToken(string token)
		{
			var user = _context.Users.FirstOrDefault(u =>
				u.Id == u.RefreshTokens.FirstOrDefault(t => t.Token == token).UserId);
			return user;
		}

		public RefreshToken GetRefreshToken(string token)
		{
			var user = GetByRefreshToken(token);
			var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token);
			return refreshToken;
		}

		public IEnumerable<ApplicationUser> GetUsers() => _context.Users;

		public async Task<bool> RemoveOutdatedRefreshTokensAsync(ApplicationUser user)
		{
			user.RefreshTokens.RemoveAll(t => !t.IsActive && t.Expires <= DateTime.UtcNow);
			var removed = await _context.SaveChangesAsync();
			return removed > 0;
		}

		public async Task<bool> RevokeRefreshTokenAsync(RefreshToken token, string reason = null,
			string replacedByToken = null)
		{
			var user = await _context.Users.FindAsync(token.UserId);
			if (user == null)
				return false;

			RevokeToken(token, reason, replacedByToken);
			_context.Update(user);
			var revoked = await _context.SaveChangesAsync();
			return revoked > 0;
		}

		public async Task<bool> RevokeRefreshTokenDescendantsAsync(RefreshToken token,
			ApplicationUser user, string reason)
		{
			// recursively traverse the refresh token chain and ensure all descendants are revoked
			var revoked = 0;
			if (!string.IsNullOrEmpty(token.ReplacedByToken))
			{
				var childToken = user.RefreshTokens.SingleOrDefault(x =>
					x.Token == token.ReplacedByToken);

				if (childToken == null || childToken.IsActive) return false;

				if (childToken.IsActive)
					RevokeToken(childToken, reason);
				else
					await RevokeRefreshTokenDescendantsAsync(childToken, user, reason);

				revoked = await _context.SaveChangesAsync();
			}
			return revoked > 0;
		}

		public bool ValidateExistence(ApplicationUser user)
		{
			var userExists = _context.Users.SingleOrDefault(u =>
				u.Email == user.Email
				|| u.UserName == user.UserName);
			return userExists != null;
		}

		private static void RevokeToken(RefreshToken token, string reason = null,
			string replacedByToken = null)
		{
			token.Revoked = DateTime.UtcNow;
			token.ReasonRevoked = reason;
			token.ReplacedByToken = replacedByToken;
		}
	}
}