using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WepA.Models.Entities
{
	public class ApplicationUser : IdentityUser
	{
		[StringLength(50)]
		[Required]
		public string FirstName { get; set; }

		[StringLength(50)]
		[Required]
		public string LastName { get; set; }

		[Column(TypeName = "Date")]
		public DateTime? DateOfBirth { get; set; }

		[StringLength(250)]
		public string Address { get; set; }

		public List<RefreshToken> RefreshTokens { get; set; }
	}
}