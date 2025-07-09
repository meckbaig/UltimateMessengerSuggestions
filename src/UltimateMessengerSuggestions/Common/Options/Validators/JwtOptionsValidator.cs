using Microsoft.Extensions.Options;
using System.Text;

namespace UltimateMessengerSuggestions.Common.Options.Validators;

sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
	public ValidateOptionsResult Validate(string? name, JwtOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{JwtOptions.ConfigurationSectionName}' must not be null.");
		}

		if (string.IsNullOrWhiteSpace(options.SecretKey))
		{
			failures.AppendLine($"'{JwtOptions.ConfigurationSectionName}:" +
				$"{nameof(JwtOptions.SecretKey)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.Issuer))
		{
			failures.AppendLine($"'{JwtOptions.ConfigurationSectionName}:" +
				$"{nameof(JwtOptions.Issuer)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.Audience))
		{
			failures.AppendLine($"'{JwtOptions.ConfigurationSectionName}:" +
				$"{nameof(JwtOptions.Audience)}' cannot be null or empty.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
