using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace WepA.Models.Dtos.Common
{
	public class CreateUserRequest
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
		public string Password { get; set; }

		[Required]
		public string ConfirmPassword { get; set; }

		public List<string> Roles { get; set; }
	}
}