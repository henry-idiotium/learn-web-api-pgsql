namespace WepA.Models.Dtos.Authenticate
{
	public class VerifyEmailRequest
	{
		public string UserId { get; set; }
		public string Code { get; set; }
	}
}