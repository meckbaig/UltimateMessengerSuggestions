using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace UltimateMessengerSuggestions.Common.Options.Validators;

sealed class ApplicationOptionsValidator : IValidateOptions<ApplicationOptions>
{
	public ValidateOptionsResult Validate(string? name, ApplicationOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{ApplicationOptions.ConfigurationSectionName}' must not be null.");
		}

		if (options.CheckDbRetryCount <= 0)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.CheckDbRetryCount)}' must be greater than 0.");
		}
		if (options.CheckDbRetryDelay <= 0)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.CheckDbRetryDelay)}' must be greater than 0.");
		}
		if (options.KnownProxies == null)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.KnownProxies)}' must not be null.");
		}
		else
		{
			foreach (var proxy in options.KnownProxies)
			{
				if (!IPAddress.TryParse(proxy, out _))
				{
					failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
						$"{nameof(ApplicationOptions.KnownProxies)}' contains not valid ip address ({proxy}).");
				}
			}
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
