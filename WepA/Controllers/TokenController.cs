using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WepA.Helpers;
using WepA.Helpers.Attributes;
using WepA.Helpers.ResponseMessages;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Common;
using WepA.Models.Dtos.Token;

namespace WepA.Controllers
{
	[JwtAuthorize]
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class TokenController : ControllerBase
	{
		private readonly IUserService _userService;

		public TokenController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpPut]
		public IActionResult Revoke(string refreshToken)
		{
			if (string.IsNullOrEmpty(refreshToken))
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidTokens);

			_userService.RevokeRefreshToken(refreshToken);
			return Ok(new GenericResponse(SuccessResponseMessages.TokenRevoked));
		}

		[AllowAnonymous]
		[HttpPut]
		public async Task<IActionResult> Rotate([FromBody] TokenRotateRequest model)
		{
			if (model.AccessToken == null || model.RefreshToken == null)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			var response = await _userService.RotateTokensAsync(model);
			return Ok(new GenericResponse(response, SuccessResponseMessages.Generic));
		}
	}
}