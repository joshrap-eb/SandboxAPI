using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SandboxAPI.Configuration;
using SandboxAPI.Filters;
using SandboxAPI.Middleware;
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

			services.AddAuthentication( options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				} )
				.AddJwtBearer( token =>
				{
					token.TokenValidationParameters = new TokenValidationParameters
					{
						IssuerSigningKeyResolver = ( s, securityToken, identifier, parameters ) =>
						{
							// get JsonWebKeySet from AWS
							var json = new WebClient( ).DownloadString( parameters.ValidIssuer +
							                                            "/.well-known/jwks.json" );
							// serialize the result
							var keys = JsonConvert.DeserializeObject<JsonWebKeySet>( json ).Keys;
							// cast the result to be the type expected by IssuerSigningKeyResolver
							return ( IEnumerable<SecurityKey> ) keys;
						},
				
						ValidIssuer = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_cc7HM6r2a",
						ValidateIssuerSigningKey = false,
						ValidateIssuer = false,
						ValidateLifetime = false,
						ValidateAudience = false
					};
				} )
				;
			services.AddHttpContextAccessor( );

			services.AddRouting( options => options.LowercaseUrls = true );
			services.AddMvc( );
			services.AddSingleton<IMovieService, MovieService>( );
			services.AddSingleton<ICognitoService, CognitoService>( );
			services.AddVersionedApiExplorer(
				options =>
				{
					options.GroupNameFormat = "'v'VVV";

					options.SubstituteApiVersionInUrl = true;
				} );

			services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerDocs>( );
			services.AddSwaggerGen( options =>
			{
				// allows swagger to generate documentation based on summaries of methods/models
				var xmlFile = $"{Assembly.GetExecutingAssembly( ).GetName( ).Name}.xml";
				var xmlPath = Path.Combine( AppContext.BaseDirectory, xmlFile );
				options.IncludeXmlComments( xmlPath );

				// include auth headers for methods requiring authentication
				options.OperationFilter<AuthorizationHeaderParameterOperationFilter>( );
			} );
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure( IApplicationBuilder app, IWebHostEnvironment env,
			IApiVersionDescriptionProvider apiVersionDescriptionProvider )
		{
			if ( env.IsDevelopment( ) )
			{
				app.UseDeveloperExceptionPage( );
			}

			app.UseHttpsRedirection( );

			app.UseRouting( );

			app.UseStaticFiles( );

			app.UseSwagger( options => { options.RouteTemplate = "/docs/{documentName}/docs.json"; } );

			// comment this out to only include the swagger docs as setup by line above
			app.UseSwaggerUI( options =>
			{
				options.DocumentTitle = "Sandbox API";
				options.RoutePrefix = "docs"; // hosted at /docs/.../ instead of /swagger/.../

				options.InjectStylesheet( "/swagger-ui/custom.css" );
				foreach ( var description in apiVersionDescriptionProvider.ApiVersionDescriptions )
				{
					options.SwaggerEndpoint( $"/docs/" + $"{description.GroupName}/docs.json",
						"Sandbox API " + description.GroupName.ToUpperInvariant( ) );
				}
			} );

			app.UseAuthentication( );
			app.UseMiddleware<AuthorizationMiddleware>( );
			app.UseAuthorization( );
			app.UseEndpoints( endpoints => { endpoints.MapControllers( ); } );
		}
	}
}