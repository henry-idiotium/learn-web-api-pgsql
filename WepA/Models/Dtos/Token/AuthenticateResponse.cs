using WepA.Models.Entities;

namespace WepA.Models.Dtos.Token
{
	public class AuthenticateResponse
	{
		public AuthenticateResponse(AccessToken accessToken, string refreshToken, ApplicationUser user)
		{
			AccessToken = accessToken;
			RefreshToken = refreshToken;
			UserInfo = new Info(id: user.Id, email: user.Email);
		}

		public AccessToken AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public Info UserInfo { get; set; }

		public class Info
		{
			public Info(string id, string email)
			{
				Id = id;
				Email = email;
			}

			public string Id { get; set; }
			public string Email { get; set; }
		}
	}
}