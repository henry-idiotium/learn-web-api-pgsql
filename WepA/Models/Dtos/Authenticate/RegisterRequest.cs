using System;
using System.ComponentModel.DataAnnotations;
using Mapster;

namespace WepA.Models.Dtos.Token
{
	public class RegisterRequest
	{
		public string UserName { get; set; }

		[Required]
		public string Email { get; set; }

		public string Address { get; set; }
		public DateTime? DateOfBirth { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		[AdaptIgnore]
		public string Password { get; set; }

		[Required]
		[AdaptIgnore]
		public string ConfirmPassword { get; set; }
	}
}