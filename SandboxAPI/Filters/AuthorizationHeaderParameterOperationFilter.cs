using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SandboxAPI.Filters
{
	public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
	{
		public void Apply( OpenApiOperation operation, OperationFilterContext context )
		{
			context.ApiDescription.TryGetMethodInfo( out var methodInfo );

			if ( methodInfo.DeclaringType == null )
				return;

			var hasAuthorizeAttribute = false;

			if ( methodInfo.MemberType == MemberTypes.Method )
			{
				// check if controller requires authentication header
				hasAuthorizeAttribute = methodInfo.DeclaringType.GetCustomAttributes( true )
					.OfType<AuthorizeAttribute>( ).Any( );

				// NOTE: Controller has Authorize attribute, so check the endpoint itself.
				//       Take into account the allow anonymous attribute
				if ( hasAuthorizeAttribute )
					hasAuthorizeAttribute =
						!methodInfo.GetCustomAttributes( true ).OfType<AllowAnonymousAttribute>( ).Any( );
				else
					hasAuthorizeAttribute = methodInfo.GetCustomAttributes( true ).OfType<AuthorizeAttribute>( ).Any( );
			}

			if ( !hasAuthorizeAttribute )
				return;

			// since we are adding the auth header, add new response types that are possible from authentication errors
			if ( operation.Responses.All( r => r.Key != StatusCodes.Status401Unauthorized.ToString( ) ) )
				operation.Responses.Add( StatusCodes.Status401Unauthorized.ToString( ),
					new OpenApiResponse {Description = "Unauthorized"} );
			if ( operation.Responses.All( r => r.Key != StatusCodes.Status403Forbidden.ToString( ) ) )
				operation.Responses.Add( StatusCodes.Status403Forbidden.ToString( ),
					new OpenApiResponse {Description = "Forbidden"} );

			operation.Parameters.Add( new OpenApiParameter
			{
				Name = "Authorization",
				In = ParameterLocation.Header,
				Description = "Access token",
				Required = true,
				Schema = new OpenApiSchema
				{
					Type = "string",
					Default = new OpenApiString( "Bearer " )
				}
			} );
		}
	}
}