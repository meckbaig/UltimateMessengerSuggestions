using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.RegularExpressions;

namespace UltimateMessengerSuggestions.Common.Conventions;

/// <summary>
/// Replaces the controller name in route templates with its camelCase version.
/// </summary>
public class CamelCaseControllerNameConvention : IControllerModelConvention
{
	/// <summary>
	/// Applies the camelCase naming convention to the controller's route templates.
	/// </summary>
	/// <param name="controller">The controller model being configured.</param>
	public void Apply(ControllerModel controller)
	{
		var originalName = controller.ControllerName;
		var camelCaseName = ToCamelCase(originalName);

		foreach (var selector in controller.Selectors)
		{
			if (selector.AttributeRouteModel != null)
			{
				selector.AttributeRouteModel.Template =
					selector.AttributeRouteModel.Template?.Replace("[controller]", camelCaseName);
			}
		}
	}

	/// <summary>
	/// Converts a PascalCase string to camelCase.
	/// </summary>
	/// <param name="input">The string to convert.</param>
	/// <returns>The camelCase version of the input.</returns>
	private static string ToCamelCase(string input)
	{
		if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
			return input;

		return char.ToLowerInvariant(input[0]) + input.Substring(1);
	}
}
