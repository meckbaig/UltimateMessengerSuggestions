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
		}
	}
}
