namespace UltimateMessengerSuggestions.Common.Options;

sealed class WebDavOptions
{
	public const string ConfigurationSectionName = "WebDav";

	/// <summary>
	/// Endpoint URL for the WebDAV server.
	/// </summary>
	public required string Endpoint { get; set; }

	/// <summary>
	/// Username for authenticating with the WebDAV server.
	/// </summary>
	public required string Username { get; set; }

	/// <summary>
	/// Password for authenticating with the WebDAV server.
	/// </summary>
	public required string Password { get; set; }

	/// <summary>
	/// Base URL for the public preview of the WebDAV server.
	/// </summary>
	public required string PublicPreviewBase { get; set; }
}
