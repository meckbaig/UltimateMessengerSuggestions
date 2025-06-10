namespace UltimateMessengerSuggestions.Common.Options;

sealed class ConnectionStringsOptions
{
	public const string ConfigurationSectionName = "ConnectionStrings";

	/// <summary>
	/// Database connection string for the application.
	/// </summary>
	public required string Default { get; set; }
}
