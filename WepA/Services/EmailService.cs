using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WepA.Helpers;
using WepA.Helpers.ResponseMessages;
using WepA.Helpers.Settings;
using WepA.Interfaces.Services;
using WepA.Models.Entities;

namespace WepA.Services
{
	public class EmailService : IEmailService
	{
		private const string _authPath = "http://localhost:3000/auth/verifyemail";
		private readonly ILogger<EmailService> _logger;
		private readonly SendGridSettings _sendGridSettings;

		public EmailService(IOptions<SendGridSettings> sendGridSettings, ILogger<EmailService> logger)
		{
			_sendGridSettings = sendGridSettings.Value;
			_logger = logger;
		}

		public async Task SendConfirmEmailAsync(ApplicationUser user, string encodedConfirmString)
		{
			var client = new SendGridClient(_sendGridSettings.ApiKey);
			var message = new SendGridMessage();

			message.SetSubject("Welcome to WepA");
			message.SetFrom(new EmailAddress(_sendGridSettings.SenderEmail,
											 _sendGridSettings.SenderName));
			message.AddTo(new EmailAddress(user.Email, $"{user.FirstName} {user.LastName}"));
			message.SetTemplateId(_sendGridSettings.TemplateId);

			var encodedUserId = EncryptHelpers.EncodeBase64Url(user.Id);
			message.SetTemplateData(new
			{
				first_name = user.FirstName,
				url = $"{_authPath}/{encodedUserId}/{encodedConfirmString}"
			});

			// Disable tracking settings
			message.SetClickTracking(false, false);
			message.SetOpenTracking(false);
			message.SetGoogleAnalytics(false);
			message.SetSubscriptionTracking(false);

			var response = await client.SendEmailAsync(message);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError($"Failed to send an email \"{user.Email}\".", response.Headers.Warning);
				throw new HttpStatusException(HttpStatusCode.InternalServerError,
											  ErrorResponseMessages.UnexpectedError);
			}
		}
	}
}