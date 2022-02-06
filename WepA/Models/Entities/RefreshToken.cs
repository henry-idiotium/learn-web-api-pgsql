using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace WepA.Models.Entities
{
	[Owned]
	public class RefreshToken
	{
		[Key]
		[JsonIgnore]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Token { get; set; }

		[Required]
		public string UserId { get; set; }
		public DateTime Expires { get; set; }
		public DateTime Created { get; set; }
		public DateTime? Revoked { get; set; }

		[StringLength(100)]
		public string ReplacedByToken { get; set; }

		[StringLength(500)]
		public string ReasonRevoked { get; set; }

		public bool IsExpired => DateTime.UtcNow >= Expires;
		public bool IsRevoked => Revoked != null;
		public bool IsActive => !IsRevoked && !IsExpired;

		[ForeignKey("UserId")]
		public ApplicationUser ApplicationUser { get; set; }
	}
}