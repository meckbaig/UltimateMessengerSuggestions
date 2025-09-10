using Microsoft.Extensions.Options;
using System.Text;

namespace UltimateMessengerSuggestions.Common.Options.Validators;

sealed class SwaggerAuthOptionsValidator : IValidateOptions<SwaggerAuthOptions>
{
	public ValidateOptionsResult Validate(string? name, SwaggerAuthOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{SwaggerAuthOptions.ConfigurationSectionName}' must not be null.");
		}

		if (string.IsNullOrWhiteSpace(options.Username))
		{
			failures.AppendLine($"'{SwaggerAuthOptions.ConfigurationSectionName}:" +
				$"{nameof(SwaggerAuthOptions.Username)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.Password))
		{
			failures.AppendLine($"'{SwaggerAuthOptions.ConfigurationSectionName}:" +
				$"{nameof(SwaggerAuthOptions.Password)}' cannot be null or empty.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
