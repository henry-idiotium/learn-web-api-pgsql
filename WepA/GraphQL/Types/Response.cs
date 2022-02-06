using System;
using WepA.Helpers.ResponseMessages;

namespace WepA.GraphQL.Types
{
	public class Response<T>
	{
		public Response(string message = SuccessResponseMessages.Generic)
		{
			Message = message;
			TimeStamp = ConvertDateTime(DateTime.UtcNow);
		}

		public Response(T data, string message = SuccessResponseMessages.Generic)
		{
			Data = data;
			Message = message;
			TimeStamp = ConvertDateTime(DateTime.UtcNow);
		}

		public T Data { get; private set; }
		public string Message { get; private set; }
		public string TimeStamp { get; private set; }

		private static string ConvertDateTime(DateTime time) => time.ToString("yyyy-MM-dd HH:mm:ss tt");
	}
}