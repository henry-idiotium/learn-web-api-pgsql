using WepA.Models.Dtos.Token;
using System.Threading.Tasks;

namespace WepA.Interfaces.Services
{
	public interface IAccountService
	{
		Task<AuthenticateResponse> LoginAsync(LoginRequest account);
		Task RegisterAsync(RegisterRequest model);
		Task VerifyEmailAsync(string userId, string token);
	}
}