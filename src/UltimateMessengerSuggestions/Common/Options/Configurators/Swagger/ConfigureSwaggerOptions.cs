using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using UltimateMessengerSuggestions.Common.Filters;

namespace UltimateMessengerSuggestions.Common.Options.Configurators.Swagger;

/// <summary>
/// Configures Swagger generation options for the application.
/// </summary>
/// <remarks>
/// This class uses the provided <see cref="IApiVersionDescriptionProvider"/> to iterate over all API version descriptions
/// and add Swagger documentation for each API version.
/// </remarks>
/// <seealso cref="IConfigureOptions{SwaggerGenOptions}"/>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
	/// <summary>
	/// Configures Swagger generation options for documenting API versions.
	/// </summary>
	/// <param name="options">Swagger generation options to configure.</param>
	public void Configure(SwaggerGenOptions options)
	{
		foreach (var description in provider.ApiVersionDescriptions)
		{
			options.SwaggerDoc(description.GroupName, new OpenApiInfo
			{
				Title = Assembly.GetExecutingAssembly().GetName().Name,
				Version = description.GroupName,
				Description = "API for suggestions during typing in messengers.",
				Contact = new OpenApiContact()
				{
					Name = "meckbaig"
				}
			});
			options.ParameterFilter<CamelCaseQueryParameterFilter>(); 
			options.DocumentFilter<RemoveSwaggerIgnoredParamsDocumentFilter>();

			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
			{
				In = ParameterLocation.Header,
				Description = "Provide a valid token",
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				BearerFormat = "JWT",
				Scheme = "Bearer"
			});
			options.AddSecurityRequirement(new OpenApiSecurityRequirement()
			{
				{
					new OpenApiSecurityScheme()
					{
						Reference = new OpenApiReference()
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			});

			var resolved = new Dictionary<Type, string>();
			var used = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

			options.CustomSchemaIds(type =>
			{
				if (resolved.TryGetValue(type, out var name))
					return name;

				string shortName = type.Name;

				if (!used.TryGetValue(shortName, out var conflict))
				{
					used[shortName] = type;
					return resolved[type] = shortName;
				}

				string Resolve(Type t) => t.DeclaringType != null ? $"{t.DeclaringType.Name}.{shortName}" : shortName;

				string prevName = Resolve(conflict);
				string currName = Resolve(type);

				used.Remove(shortName);
				used[prevName] = conflict;
				used[currName] = type;

				resolved[conflict] = prevName;
				resolved[type] = currName;

				return currName;
			});
		}
	}
}
