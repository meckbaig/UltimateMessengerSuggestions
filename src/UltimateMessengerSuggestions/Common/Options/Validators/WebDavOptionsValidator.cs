using Microsoft.Extensions.Options;
using System.Text;

namespace UltimateMessengerSuggestions.Common.Options.Validators;

sealed class WebDavOptionsValidator : IValidateOptions<WebDavOptions>
{
	public ValidateOptionsResult Validate(string? name, WebDavOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{WebDavOptions.ConfigurationSectionName}' must not be null.");
		}

		if (string.IsNullOrWhiteSpace(options.Endpoint))
		{
			failures.AppendLine($"'{WebDavOptions.ConfigurationSectionName}:" +
				$"{nameof(WebDavOptions.Endpoint)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.Username))
		{
			failures.AppendLine($"'{WebDavOptions.ConfigurationSectionName}:" +
				$"{nameof(WebDavOptions.Username)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.Password))
		{
			failures.AppendLine($"'{WebDavOptions.ConfigurationSectionName}:" +
				$"{nameof(WebDavOptions.Password)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.PublicPreviewBase))
		{
			failures.AppendLine($"'{WebDavOptions.ConfigurationSectionName}:" +
				$"{nameof(WebDavOptions.PublicPreviewBase)}' cannot be null or empty.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
