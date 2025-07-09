namespace UltimateMessengerSuggestions.Common.Options;

sealed class JwtOptions
{
	public const string ConfigurationSectionName = "Jwt";

	/// <summary>
	/// The secret key used for signing JWT tokens.
	/// </summary>
	public string SecretKey { get; set; } = string.Empty;

	/// <summary>
	/// The issuer of the JWT tokens.
	/// </summary>
	public string Issuer { get; set; } = string.Empty;

	/// <summary>
	/// The audience for which the JWT tokens are intended.
	/// </summary>
	public string Audience { get; set; } = string.Empty;
}
