using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WepA.Helpers;
using WepA.Helpers.ResponseMessages;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Common;
using WepA.Models.Dtos.Token;

namespace WepA.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class AuthController : ControllerBase
	{
		private readonly IAccountService _accountService;

		public AuthController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		[HttpGet]
		public async Task<IActionResult> Login([FromBody] LoginRequest model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState.SelectMany(k => k.Value.Errors));

			var authenticateResponse = await _accountService.LoginAsync(model);

			return Ok(new GenericResponse(authenticateResponse, SuccessResponseMessages.Generic));
		}

		[HttpPost]
		public async Task<IActionResult> Register([FromBody] RegisterRequest model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState.SelectMany(x => x.Value.Errors));

			await _accountService.RegisterAsync(model);

			return Ok(new GenericResponse(SuccessResponseMessages.UserRegistered));
		}

		[HttpPut("{userId}/{code}")]
		public async Task<IActionResult> VerifyEmail(string userId, string code)
		{
			var decodedUserId = EncryptHelpers.DecodeBase64Url(userId);
			var decodedConfirmString = EncryptHelpers.DecodeBase64Url(code);
			await _accountService.VerifyEmailAsync(decodedUserId, decodedConfirmString);
			return Ok(new GenericResponse(SuccessResponseMessages.EmailVerified));
		}
	}
}