using System.Threading.Tasks;
using WepA.Models.Entities;

namespace WepA.Interfaces.Services
{
	public interface IEmailService
	{
		Task SendConfirmEmailAsync(ApplicationUser user, string encodedConfirmString);
	}
}