using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SandboxAPI.Services;

namespace SandboxAPI.Controllers
{
	[ApiController]
	[ApiVersion( "1" )]
	[Route( "api/v{version:apiVersion}/[controller]" )]
	public class CognitoController
	{
		private readonly ILogger<CognitoController> _logger;

		private readonly ICognitoService _cognitoService;

		public CognitoController( ILogger<CognitoController> logger, ICognitoService cognitoService )
		{
			_cognitoService = cognitoService;
			_logger = logger;
		}

		/// <summary>
		/// Initiates authentication with AWS Cognito
		/// </summary>
		/// <returns>Refresh, access, and id tokens for the authenticated user</returns>
		/// <response code="200">User signed in successfully.</response>
		/// <response code="404">User cannot sign in</response>
		/// <response code="500">Server cannot process your request.</response>
		[ProducesResponseType( typeof( string ), 200 )]
		[ProducesResponseType( 400 )]
		[ProducesResponseType( 500 )]
		[Route( "signIn" )]
		[HttpPost]
		public ActionResult<string> SignIn( string username, string password )
		{
			return _cognitoService.SignIn( username, password ).Result;
		}
	}
}