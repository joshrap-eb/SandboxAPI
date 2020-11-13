using System.Threading.Tasks;

namespace SandboxAPI.Services
{
	public interface ICognitoService
	{
		public Task<string> SignIn( string username, string password );
		
	}
}