using System.Threading;
using System.Threading.Tasks;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace WepA.Models.Dtos.Common
{
	public class HttpRequestInterceptor : DefaultHttpRequestInterceptor
	{
		private readonly IPolicyEvaluator _policyEvaluator;
		private readonly IAuthorizationPolicyProvider _policyProvider;

		public HttpRequestInterceptor(IPolicyEvaluator policyEvaluator,
		   IAuthorizationPolicyProvider policyProvider)
		{
			_policyEvaluator = policyEvaluator;
			_policyProvider = policyProvider;
		}

		public override async ValueTask OnCreateAsync(HttpContext context,
			IRequestExecutor requestExecutor, IQueryRequestBuilder requestBuilder,
			CancellationToken cancellationToken)
		{
			await _policyEvaluator.AuthenticateAsync(await _policyProvider.GetDefaultPolicyAsync(), context);
			await base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
		}
	}
}