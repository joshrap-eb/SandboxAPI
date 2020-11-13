using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SandboxAPI.Configuration
{
	public class ConfigureSwaggerDocs : IConfigureOptions<SwaggerGenOptions>
	{
		private readonly IApiVersionDescriptionProvider _provider;

		public ConfigureSwaggerDocs( IApiVersionDescriptionProvider provider ) => _provider = provider;

		public void Configure( SwaggerGenOptions options )
		{
			foreach ( var description in _provider.ApiVersionDescriptions )
			{
				// add a swagger doc for each api version found
				options.SwaggerDoc( description.GroupName, CreateInfoForApiVersion( description ) );
			}
		}

		private static OpenApiInfo CreateInfoForApiVersion( ApiVersionDescription description )
		{
			var info = new OpenApiInfo( )
			{
				Title = "Sandbox API",
				Description = "Sandbox API to test using Swagger.",
				Contact = new OpenApiContact
				{
					Name = "Joshua Rapoport", Email = "joshua.rapoport@eventbooking.com",
					Url = new Uri( "https://www.google.com" )
				},
				Version = description.ApiVersion.ToString( ),
			};

			if ( description.IsDeprecated )
			{
				info.Description += " This API version has been deprecated.";
			}

			return info;
		}
	}
}