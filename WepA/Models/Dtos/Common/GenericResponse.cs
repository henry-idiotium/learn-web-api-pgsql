using System;
using System.Text.Json.Serialization;

namespace WepA.Models.Dtos.Common
{
	public class GenericResponse
	{
		public GenericResponse() { }

		public GenericResponse(string message, bool succeeded = true)
		{
			Succeeded = succeeded;
			Message = message;
			TimeStamp = ConvertDateTime(DateTime.UtcNow);
		}

		public GenericResponse(object data, string message, bool succeeded = true)
		{
			Data = data;
			Message = message;
			Succeeded = succeeded;
			TimeStamp = ConvertDateTime(DateTime.UtcNow);
		}

		public bool Succeeded { get; private set; }

		public string Message { get; private set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public object Data { get; private set; }

		public string TimeStamp { get; private set; }
		static string ConvertDateTime(DateTime time) => time.ToString("yyyy-MM-dd HH:mm:ss tt");
	}
}