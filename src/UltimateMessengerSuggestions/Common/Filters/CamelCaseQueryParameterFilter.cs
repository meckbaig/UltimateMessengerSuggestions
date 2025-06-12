using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace UltimateMessengerSuggestions.Common.Filters;

/// <summary>
/// A filter that converts query parameter names to camelCase.
/// </summary>
public class CamelCaseQueryParameterFilter : IParameterFilter
{
	/// <summary>
	/// Applies the camelCase naming convention to query parameter names.
	/// </summary>
	/// <param name="parameter"></param>
	/// <param name="context"></param>
	public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
	{
		var originalName = parameter.Name;
		var camelCaseName = JsonNamingPolicy.CamelCase.ConvertName(originalName);
		parameter.Name = camelCaseName;
	}
}
