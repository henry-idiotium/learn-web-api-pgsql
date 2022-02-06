using System.Collections.Generic;
using System.Net;
using HotChocolate;
using WepA.Helpers.ResponseMessages;

namespace WepA.Helpers
{
	public class GraphQLErrorFilter : IErrorFilter
	{
		public IError OnError(IError error)
		{
			var (code, message) = ParseError(error);
			return ErrorBuilder.FromError(error)
				.SetMessage(message).SetCode(code)
				.RemoveException().ClearExtensions()
				.RemovePath().ClearLocations()
				.Build();
		}

		private static (string Code, string Message) ParseError(IError error)
		{
			var dict = new Dictionary<HttpStatusCode, string>
			{
				{ HttpStatusCode.InternalServerError, ErrorCodes.Server.RequestInvalid },
				{ HttpStatusCode.Unauthorized, ErrorCodes.Authentication.NotAuthorized },
				{ HttpStatusCode.BadRequest, ErrorCodes.Server.RequestInvalid },
			};

			var code = error.Exception == null
				? !(error.Code == ErrorCodes.Authentication.NotAuthorized ||
					error.Code == ErrorCodes.Authentication.NotAuthenticated)
					? ErrorCodes.Server.RequestInvalid
					: error.Code
				: dict[
					error.Exception is HttpStatusException ex
						? ex.Status
						: HttpStatusCode.InternalServerError
				];

			var message = !(error.Code == ErrorCodes.Authentication.NotAuthorized ||
							error.Code == ErrorCodes.Authentication.NotAuthenticated)
				? error.Exception is HttpStatusException
					? error.Exception.Message
					: ErrorResponseMessages.UnexpectedError
				: ErrorResponseMessages.Unauthorized;

			return (code, message);
		}
	}
}