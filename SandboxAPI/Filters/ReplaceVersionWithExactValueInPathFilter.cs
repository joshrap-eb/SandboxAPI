﻿using System;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SandboxAPI.Filters
{
	public class ReplaceVersionWithExactValueInPathFilter :IDocumentFilter
	{
		public void Apply( OpenApiDocument swaggerDoc, DocumentFilterContext context )
		{
			var paths = new OpenApiPaths();
			foreach (var (key, value) in swaggerDoc.Paths)
			{
				paths.Add(key.Replace("v{version}", swaggerDoc.Info.Version, StringComparison.InvariantCulture), value);
			}
			swaggerDoc.Paths = paths;
		}
	}
}