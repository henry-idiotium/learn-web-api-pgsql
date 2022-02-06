using System;
using System.Text.Json.Serialization;
using Mapster;
using Sieve.Attributes;

namespace WepA.Models.Dtos.Common
{
	public class UserDetailsResponse
	{
		[Sieve(CanFilter = false, CanSort = false)]
		public string Id { get; set; }

		[Sieve(CanFilter = true, CanSort = true)]
		public string UserName { get; set; }

		[Sieve(CanFilter = true, CanSort = true)]
		public string Email { get; set; }

		[Sieve(CanFilter = true, CanSort = true)]
		public string FirstName { get; set; }

		[Sieve(CanFilter = true, CanSort = true)]
		public string LastName { get; set; }

		[Sieve(CanFilter = true, CanSort = true)]
		public string Address { get; set; }

		[JsonIgnore]
		[Sieve(CanFilter = true, CanSort = true)]
		public DateTime? DateOfBirth { get; set; }

		[AdaptIgnore]
		[JsonPropertyName("dateOfBirth")]
		public string DateOfBirthString { get { return DateOfBirth?.ToString("dd-MM-yyyy"); } }
	}
}