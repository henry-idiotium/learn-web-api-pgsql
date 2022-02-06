namespace WepA.Helpers.ResponseMessages
{
	public static class ErrorResponseMessages
	{
		public const string EmailNotVerify = "Please verify email.";
		public const string FailedLogin = "Email or password is incorrect.";
		public const string FailedVerifyEmail = "Failed to verify email.";
		public const string InvalidRequest = "Invalid request.";
		public const string InvalidTokens = "Invalid tokens.";
		public const string NotFoundUser = "User not found.";
		public const string PasswordNotMatch = "Submitted passwords didn't match.";
		public const string Unauthenticated = "Failed to authenticate with this resource.";
		public const string Unauthorized = "Unauthorized to access this resource.";
		public const string UnexpectedError = "Something went wrong.";
		public const string UserAlreadyExists = "This username or email already exists.";
	}
}