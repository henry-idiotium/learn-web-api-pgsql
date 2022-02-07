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
		public async Task<IActionResult> Create(CreateUserRequest model)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			await _userService.CreateAsync(model);
			return Ok(new GenericResponse(SuccessResponseMessages.UserRegistered));
		}

		[HttpGet]
		public async Task<IActionResult> GetDetails(string userId)
		{
			var decodedUserId = EncryptHelpers.DecodeBase64Url(userId);
			var user = await _userService.GetByIdAsync(decodedUserId);
			return Ok(new GenericResponse(user, SuccessResponseMessages.Generic));
		}

		[HttpPost]
		public IActionResult GetList(SieveModel model)
		{
			var users = _userService.GetList(model);
			return Ok(new GenericResponse(users, SuccessResponseMessages.Generic));
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> MockCreate(List<CreateUserRequest> models)
		{
			await _userService.MockCreateAsync(models);
			return Ok(new GenericResponse(SuccessResponseMessages.UserRegistered));
		}
	}
}