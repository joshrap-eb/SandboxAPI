using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SandboxAPI.Filters;
using SandboxAPI.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SandboxAPI
{
	public class Startup
	{
		public Startup( IConfiguration configuration )
		{
			Configuration = configuration;
		}

		private IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices( IServiceCollection services )
		{
			services.AddControllers( );
			services.AddApiVersioning( o =>
			{
				o.AssumeDefaultVersionWhenUnspecified = true;
				o.DefaultApiVersion = new ApiVersion( 1, 0 );
			} );

			services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme ).AddJwtBearer( options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
					{
						// get JsonWebKeySet from AWS
						var json = new WebClient().DownloadString(parameters.ValidIssuer + "/.well-known/jwks.json");
						// serialize the result
						var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;
						// cast the result to be the type expected by IssuerSigningKeyResolver
						return (IEnumerable<SecurityKey>)keys;
					},

					ValidIssuer = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_BTDPXcMc7",
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateLifetime = true,
					// ValidAudience = "1ukd4ljadtnubo0nc5bfui3b07",
					// ValidateAudience = true
				};
			} );

			services.AddRouting( options => options.LowercaseUrls = true );
			services.AddMvc( );
			services.AddSingleton<IMovieService, MovieService>( );
			services.AddSingleton<ICognitoService, CognitoService>( );

			services.AddSwaggerGen( options =>
			{
				options.SwaggerDoc( "v1", new OpenApiInfo
				{
					Title = "Sandbox API v1",
					Version = "v1",
					Description = "Sandbox API to test using Swagger.",
					Contact = new OpenApiContact
					{
						Email = "joshua.rapoport@evenbooking.com",
						Name = "Joshua Rapoport"
					}
				} );
				options.SwaggerDoc( "v2", new OpenApiInfo
				{
					Title = "Sandbox API v2",
					Version = "v2",
					Description = "Sandbox API to test using Swagger.",
					Contact = new OpenApiContact
					{
						Email = "joshua.rapoport@evenbooking.com",
						Name = "Joshua Rapoport"
					}
				} ); // these versions must match the api versions for custom filter to work
				// allows swagger to generate documentation based on summaries of methods/models
				var xmlFile = $"{Assembly.GetExecutingAssembly( ).GetName( ).Name}.xml";
				var xmlPath = Path.Combine( AppContext.BaseDirectory, xmlFile );
				options.IncludeXmlComments( xmlPath );
				
				// include auth headers for methods requiring authentication
				options.OperationFilter<AuthorizationHeaderParameterOperationFilter>( );

				options.OperationFilter<RemoveVersionParameterFilter>( );
				options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>( );
				options.DocInclusionPredicate( ( version, desc ) =>
				{
					// updates the swagger UI to display methods to belong to their respective API version
					if ( !desc.TryGetMethodInfo( out var methodInfo ) )
						return false;

					var versions = methodInfo.DeclaringType?.GetCustomAttributes( true )
						.OfType<ApiVersionAttribute>( )
						.SelectMany( attr => attr.Versions );

					var maps = methodInfo
						.GetCustomAttributes( true )
						.OfType<MapToApiVersionAttribute>( )
						.SelectMany( attr => attr.Versions )
						.ToArray( );

					return versions.Any( v => $"v{v.ToString( )}" == version )
					       && ( !maps.Any( ) || maps.Any( v => $"v{v.ToString( )}" == version ) );
				} );
			} );
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
		{
			if ( env.IsDevelopment( ) )
			{
				app.UseDeveloperExceptionPage( );
			}

			app.UseHttpsRedirection( );

			app.UseRouting( );

			app.UseStaticFiles( );

			app.UseSwagger( options => { options.RouteTemplate = "/docs/{documentName}/api-doc.json"; } );

			app.UseSwaggerUI( options =>
			{
				options.DocumentTitle = "Sandbox API";
				options.RoutePrefix = "docs";

				options.InjectStylesheet( "/swagger-ui/custom.css" );
				// endpoints must match template defined in UseSwagger method
				options.SwaggerEndpoint( "/docs/v1/api-doc.json", "Sandbox API V1" );
				options.SwaggerEndpoint( "/docs/v2/api-doc.json", "Sandbox API V2" );
			} );

			app.UseAuthentication( );

			app.UseAuthorization( );

			app.UseEndpoints( endpoints => { endpoints.MapControllers( ); } );
		}
	}
}