using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace UltimateMessengerSuggestions.Common.Filters;

public class RemoveSwaggerIgnoredParamsDocumentFilter : IDocumentFilter
{
	public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
	{
		foreach (var path in swaggerDoc.Paths)
		{
			foreach (var operation in path.Value.Operations.Values)
			{
				var toRemove = operation.Parameters
					.Where(p => IsIgnoredParameter(p, context))
					.ToList();

				foreach (var param in toRemove)
				{
					operation.Parameters.Remove(param);
				}
			}
		}
	}

	private bool IsIgnoredParameter(OpenApiParameter parameter, DocumentFilterContext context)
	{
		if (parameter == null || string.IsNullOrEmpty(parameter.Name))
			return false;

		foreach (var apiDesc in context.ApiDescriptions)
		{
			foreach (var paramDesc in apiDesc.ParameterDescriptions)
			{
				var paramType = paramDesc.ParameterDescriptor?.ParameterType;
				if (paramType == null)
					continue;

				bool? result = FindPropertyRecursive(paramType, parameter.Name);
				if (result != null)
					return (bool)result;
			}
		}

		return false;
	}

	private static bool? FindPropertyRecursive(Type rootType, string dottedName)
	{
		var segments = dottedName.Split('.', StringSplitOptions.RemoveEmptyEntries);
		Type currentType = rootType;
		PropertyInfo? currentProp = null;

		foreach (var segment in segments)
		{
			currentProp = currentType.GetProperty(segment, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			if (currentProp == null)
				return null;

			currentType = currentProp.PropertyType;
			var hasIgnore = currentProp.GetCustomAttribute<SwaggerIgnoreAttribute>() != null;
			if (hasIgnore)
				return true;
		}

		return null;
	}
}
