using System;
using System.Linq;
using System.Net;
using System.Reflection;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WepA.Helpers;
using WepA.Helpers.ResponseMessages;
using WepA.Interfaces.Services;
using WepA.Services;

namespace WepA.Models.Dtos.Common
{
	[AttributeUsage(
		AttributeTargets.Class | AttributeTargets.Method,
		Inherited = true,
		AllowMultiple = true)]
	public class GqlAuthorizeAttribute : ObjectFieldDescriptorAttribute
	{
		public override void OnConfigure(IDescriptorContext context,
			IObjectFieldDescriptor descriptor, MemberInfo member)
		{
			var httpContextAccessor = context.Services.GetService(typeof(IHttpContextAccessor))
				as IHttpContextAccessor;

			var authorizationHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"]
				.FirstOrDefault()?
				.Split(" ")
				.Last();

			var jwtService = context.Services.GetService(typeof(JwtService)) as IJwtService;
			// var jwtService = ServiceProvider.GetService(typeof(JwtService)) as IJwtService;

			var userId = jwtService.Validate(authorizationHeader);

			if (userId == null)
				throw new GraphQLException(ErrorBuilder.New()
					.SetMessage(ErrorResponseMessages.Unauthenticated)
					.SetCode(ErrorCodes.Authentication.NotAuthenticated)
					.RemoveException().ClearExtensions()
					.RemovePath().ClearLocations()
					.Build());

			return;
		}
	}
}