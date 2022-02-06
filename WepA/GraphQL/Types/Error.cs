using System.Net;

namespace WepA.GraphQL.Types
{
	public class Error
	{
		public Error(string message)
		{
			Message = message;
		}

		public string Message { get; set; }
	}
}