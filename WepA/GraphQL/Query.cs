using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using MapsterMapper;
using Sieve.Models;
using WepA.GraphQL.Types;
using WepA.Helpers;
using WepA.Helpers.ResponseMessages;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Authenticate;

namespace WepA.GraphQL
{
	public class Query
	{
		public async Task<Response<UserDetails>> GetAuthInfoAsync(
			[Service] IUserService userService,
			[Service] IMapper mapper,
			AuthInfoRequest request)
		{
			var response = await userService.GetByIdAsync(request.UserId);
			if (response == null)
				throw new HttpStatusException(HttpStatusCode.BadRequest,
											  ErrorResponseMessages.InvalidRequest);

			return new(mapper.Map<UserDetails>(response));
		}

		[Authorize]
		public Response<ResponseTable<UserDetails>> GetUsers(
			[Service] IUserService userService,
			[Service] IMapper mapper,
			SieveModel request)
		{
			var users =  userService.GetList(request);
			return new(new ResponseTable<UserDetails>(
				count: users.Count,
				currentPage: users.CurrentPage,
				totalPages: users.TotalPages,
				rows: mapper.Map<IEnumerable<UserDetails>>(users.Rows)));
		}
	}
}