using System.Collections.Generic;

namespace WepA.GraphQL.Types
{
	public class ResponseTable<T>
	{
		public ResponseTable(IEnumerable<T> rows, int count, int currentPage, int totalPages)
		{
			Rows = rows;
			Count = count;
			CurrentPage = currentPage;
			TotalPages = totalPages;
		}

		public int Count { get; private set; }
		public IEnumerable<T> Rows { get; private set; }
		public int CurrentPage { get; private set; }
		public int TotalPages { get; private set; }
	}
}