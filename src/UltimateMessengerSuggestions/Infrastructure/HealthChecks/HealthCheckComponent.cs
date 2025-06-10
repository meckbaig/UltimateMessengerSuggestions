namespace UltimateMessengerSuggestions.Infrastructure.HealthChecks;

/// <summary>
/// Represents a component used in the application's health check.
/// </summary>
public class HealthCheckComponent
{
	/// <summary>
	/// The name of the health check component.
	/// </summary>
	public required string Entry { get; set; }

	/// <summary>
	/// The status of the component.
	/// </summary>
	public required string Status { get; set; }

	/// <summary>
	/// A description of the component's status.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// The error message if the component is in an error state.
	/// </summary>
	public string? ErrorMessage { get; set; }
}
