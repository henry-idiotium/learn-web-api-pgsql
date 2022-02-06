namespace WepA.Models.Dtos.Token
{
	public class AccessToken
	{
		public AccessToken(string token, string expireAt)
		{
			Token = token;
			ExpireAt = expireAt;
		}

		public string Token { get; set; }
		public string ExpireAt { get; set; }
	}
}