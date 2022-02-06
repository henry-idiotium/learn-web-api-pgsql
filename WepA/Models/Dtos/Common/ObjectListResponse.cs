using System.Collections.Generic;

namespace WepA.Models.Dtos.Common
{
	public class ObjectListResponse
	{
		public ObjectListResponse(IEnumerable<object> rows, int count, int currentPage, int totalPages)
		{
			Rows = rows;
			Count = count;
			CurrentPage = currentPage;
			TotalPages = totalPages;
		}

		public int Count { get; private set; }
		public IEnumerable<object> Rows { get; private set; }
		public int CurrentPage { get; private set; }
		public int TotalPages { get; private set; }
	}
}