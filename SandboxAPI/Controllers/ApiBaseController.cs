using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SandboxAPI.Security;

namespace SandboxAPI.Controllers
{
	[Authorize]
	public class ApiBaseController : Controller
	{
		protected AuthParamModel AuthParamModel { get; private set; }

		public override async Task OnActionExecutionAsync( ActionExecutingContext context,
			ActionExecutionDelegate next )
		{
			await GetAuthParams( context );
			await next( );
		}

		[ApiExplorerSettings( IgnoreApi = true )]
		public async Task GetAuthParams( ActionExecutingContext context )
		{
			if ( AuthParamModel != null )
				return;

			var userClaims = context.HttpContext.User.Claims.ToList( );
			var clientId = ( from claim in userClaims where claim.Type.Equals( "client_id" ) select claim.Value )
				.FirstOrDefault( );
			// todo would actually do a raven DB call to load the permissions using the client id

			AuthParamModel = new AuthParamModel
			{
				ClientId = clientId,
				TenantId = "TenantId-1",
				WritePermissions = true,
				ReadPermissions = true
			};
		}
	}
}