using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace SandboxAPI.Middleware
{
	public class AuthorizationMiddleware
	{
		private readonly RequestDelegate _next;

		public AuthorizationMiddleware( RequestDelegate next )
		{
			_next = next;
		}

		public async Task Invoke( HttpContext context )
		{
			var identity = context.User.Identity as ClaimsIdentity;
			if ( !identity.IsAuthenticated )
			{
				// this is really just a sanity check to make sure user is authenticated.
				context.Response.StatusCode = 401;
				return;
			}

			await _next( context );
		}
	}
}