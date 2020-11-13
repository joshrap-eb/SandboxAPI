using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;

namespace SandboxAPI.Services
{
	public class CognitoService : ICognitoService
	{
		private const string ClientId = "6rr28slldfhveldjof2psom4s9";
		private const string UserPool = "us-east-1_BTDPXcMc7";

		public async Task<string> SignIn( string username, string password )
		{
			var provider =
				new AmazonCognitoIdentityProviderClient( new AnonymousAWSCredentials( ) );

			var userPool = new CognitoUserPool( UserPool, ClientId, provider );
			var user = new CognitoUser( username, ClientId, userPool, provider );
			var authRequest = new InitiateSrpAuthRequest( )
			{
				Password = password
			};

			var authResponse = await user.StartWithSrpAuthAsync( authRequest ).ConfigureAwait( false );
			// if the user has mfa enabled, this response will have a challenge attribute set accordingly. We would then need to branch based on the MFA type. The return type should probably be some type of object that has some field that says "tokenPresent"
			string retVal;
			if ( string.IsNullOrWhiteSpace( authResponse.ChallengeName ) )
			{
				retVal = authResponse.AuthenticationResult.AccessToken;
			}
			else
			{
				retVal = authResponse.ChallengeName;
			}

			return retVal;
		}
	}
}