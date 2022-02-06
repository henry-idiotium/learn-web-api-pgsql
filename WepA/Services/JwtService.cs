using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WepA.Helpers;
using WepA.Helpers.ResponseMessages;
using WepA.Helpers.Settings;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Token;
using WepA.Models.Entities;

namespace WepA.Services
{
	public class JwtService : IJwtService
	{
		private readonly byte[] _jwtEncodedSecret;
		private readonly JwtSettings _jwtSettings;
		private readonly List<string> _validLocations = new()
		{
			"http://localhost:3000",
			"https://localhost:5001",
			"https://localhost:5000"
		};
		public JwtService(IOptions<JwtSettings> jwtSettings)
		{
			_jwtSettings = jwtSettings.Value;
			_jwtEncodedSecret = EncryptHelpers.EncodeASCII(_jwtSettings.Secret);
		}

		public AccessToken GenerateAccessToken(IEnumerable<Claim> claims)
		{
			var expireAt = DateTime.UtcNow.AddDays(_jwtSettings.AccessTokenExpiredDate);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Issuer = _jwtSettings.Issuer,
				Audience = _jwtSettings.Audience,
				Subject = new ClaimsIdentity(claims),
				Expires = expireAt,
				SigningCredentials = new SigningCredentials(
					key: new SymmetricSecurityKey(_jwtEncodedSecret),
					algorithm: SecurityAlgorithms.HmacSha256Signature)
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return new AccessToken(tokenHandler.WriteToken(token), expireAt.ToString("o"));
		}

		public RefreshToken GenerateRefreshToken(string userId)
		{
			if (userId == null)
			{
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);
			}
			using var cryptoProvider = new RNGCryptoServiceProvider();
			var randomBytes = new byte[64];
			cryptoProvider.GetBytes(randomBytes);
			var refreshToken = new RefreshToken
			{
				Token = Convert.ToBase64String(randomBytes),
				Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiredDate),
				Created = DateTime.UtcNow,
				UserId = userId
			};
			return refreshToken;
		}

		public ClaimsPrincipal GetClaimsPrincipal(string token)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var jwtToken = tokenHandler.ReadToken(token);
				if (jwtToken is null) return null;

				var validationParameters = new TokenValidationParameters()
				{
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = false,

					ValidIssuers = _validLocations,
					ValidAudiences = _validLocations,
					IssuerSigningKey = new SymmetricSecurityKey(_jwtEncodedSecret)
				};

				var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
				return principal;
			}
			catch
			{
				return null;
			}
		}

		public string Validate(string token)
		{
			if (token is null) return null;
			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,

					ValidIssuers = _validLocations,
					ValidAudiences = _validLocations,
					IssuerSigningKey = new SymmetricSecurityKey(_jwtEncodedSecret),
					ClockSkew = TimeSpan.Zero
				}, out SecurityToken validatedToken);

				var jwtToken = validatedToken as JwtSecurityToken;
				var userId = jwtToken.Claims
					.FirstOrDefault(claim => (claim.Type == "nameid") && (claim.Value != null))
					.Value;
				return userId; // Upon successful validation return user id from JWT token
			}
			catch
			{
				return null; // When fails validation
			}
		}
	}
}