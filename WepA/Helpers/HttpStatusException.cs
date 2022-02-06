using System;
using System.Globalization;
using System.Net;

namespace WepA.Helpers
{
	public class HttpStatusException : Exception
	{
		public HttpStatusException(HttpStatusCode status, string message = null) : base(message)
		{
			Status = status;
		}

		public HttpStatusException(HttpStatusCode status, string message = null, params object[] args)
			: base(string.Format(CultureInfo.CurrentCulture, message, args))
		{
			Status = status;
		}

		public HttpStatusCode Status { get; private set; }
	}
}