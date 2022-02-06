using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WepA.Helpers;
using WepA.Helpers.ResponseMessages;
using WepA.Interfaces.Services;

// DEPRECATED Already implement jwt attribute
namespace WepA.Middlewares
{
	public static class JwtMiddlewareExt
	{
		public static void UseJwtExt(this IApplicationBuilder app) =>
			app.UseMiddleware<JwtMiddleware>();
	}

	public class JwtMiddleware
	{
		private readonly RequestDelegate _next;

		public JwtMiddleware(RequestDelegate next) => _next = next;

		public async Task InvokeAsync(HttpContext context,
			IUserService userService, IJwtService jwtService)
		{
			var token = context.Request.Headers["Authorization"]
				.FirstOrDefault()?
				.Split(" ")
				.Last();
			var userId = jwtService.Validate(token);
			if (!string.IsNullOrWhiteSpace(userId))
				context.Items["ApplicationUser"] = await userService.GetByIdAsync(userId);

			await _next(context);
		}
	}
}