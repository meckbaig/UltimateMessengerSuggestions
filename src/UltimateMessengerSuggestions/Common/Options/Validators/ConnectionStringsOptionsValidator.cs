using Microsoft.Extensions.Options;

namespace UltimateMessengerSuggestions.Common.Options.Validators;

sealed class ConnectionStringsOptionsValidator : IValidateOptions<ConnectionStringsOptions>
{
	public ValidateOptionsResult Validate(string? name, ConnectionStringsOptions options)
	{
		if (string.IsNullOrWhiteSpace(options.Default))
		{
			return ValidateOptionsResult.Fail("The connection line to the database should not be empty");
		}

		return ValidateOptionsResult.Success;
	}
}
