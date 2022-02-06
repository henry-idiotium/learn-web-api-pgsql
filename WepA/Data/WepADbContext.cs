using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WepA.Models.Entities;

namespace WepA.Data
{
	public class WepADbContext : IdentityDbContext<ApplicationUser>
	{
		public WepADbContext(DbContextOptions<WepADbContext> options) : base(options) { }

		public DbSet<RefreshToken> RefreshTokens { get; set; }
	}
}