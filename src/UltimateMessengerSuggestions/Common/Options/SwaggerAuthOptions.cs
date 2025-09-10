namespace UltimateMessengerSuggestions.Common.Options;

sealed class SwaggerAuthOptions
{
	public const string ConfigurationSectionName = "SwaggerAuth";

	public required string Username { get; set; }
	public required string Password { get; set; }
	public required bool RequireLogin { get; set; }
}
