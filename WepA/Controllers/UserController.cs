using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using WepA.Helpers;
using WepA.Helpers.Attributes;
using WepA.Helpers.ResponseMessages;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Common;

namespace WepA.Controllers
{
	[JwtAuthorize]
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateUserRequest model)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			await _userService.CreateAsync(model);
			return Ok(new GenericResponse(SuccessResponseMessages.UserRegistered));
		}

		[HttpGet]
		public async Task<IActionResult> GetDetails(string encodedUserId)
		{
			var userId = EncryptHelpers.DecodeBase64Url(encodedUserId);
			var user = await _userService.GetByIdAsync(userId);
			return Ok(new GenericResponse(user, SuccessResponseMessages.Generic));
		}

		[AllowAnonymous]
		[HttpGet]
		public IActionResult GetList([FromQuery] SieveModel model)
		{
			var users = _userService.GetList(model);
			return Ok(new GenericResponse(users, SuccessResponseMessages.Generic));
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> MockCreate([FromBody] List<CreateUserRequest> models)
		{
			await _userService.MockCreateAsync(models);
			return Ok(new GenericResponse(SuccessResponseMessages.UserRegistered));
		}
	}
}