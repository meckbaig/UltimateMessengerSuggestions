using System.IO;
using System.Text.Json;

namespace Meckbaig.Cqrs;

/// <summary>
/// Provides extension methods for JSON serialization and naming conventions.
/// </summary>
public static partial class JsonExtensions
{
	/// <summary>
	/// JSON naming policy used for converting property names.
	/// </summary>
	public static JsonNamingPolicy NamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;

	/// <summary>
	/// Converts the specified property name to its JSON representation using the configured naming policy.
	/// </summary>
	/// <param name="propertyName">The name of the property to convert. Cannot be null or empty.</param>
	/// <returns>The converted property name as a string, formatted according to the JSON naming policy.</returns>
	public static string Print(this string propertyName)
	{
		return NamingPolicy.ConvertName(propertyName);
	}

	/// <summary>
	/// Converts a string to pascal case.
	/// </summary>
	/// <param name="value">Input string.</param>
	/// <returns>String in pascal case.</returns>
	public static string ToPascalCase(this string value)
	{
		if (value.Length <= 1)
			return value.ToUpper();
		return $"{value[0].ToString().ToUpper()}{value.Substring(1)}";
	}
}
