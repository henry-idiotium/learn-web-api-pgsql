using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WepA.Models.Dtos.Common
{
	public class UpdateUserRequest
	{
		[JsonPropertyName("id")]
		public string EncodedId { get; set; }

		public string UserName { get; set; }

		public string Address { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		public DateTime? DateOfBirth { get; set; }

		public List<string> Roles { get; set; }
	}
}