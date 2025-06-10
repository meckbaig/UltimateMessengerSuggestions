namespace UltimateMessengerSuggestions.Infrastructure.HealthChecks;

/// <summary>
/// Represents the response of the application's health check.
/// </summary>
public class HealthCheckResponse
{
	/// <summary>
	/// Overall health status of the application.
	/// </summary>
	public required string Status { get; set; }

	/// <summary>
	/// Total duration of the health check execution in seconds.
	/// </summary>
	public double Duration { get; set; }

	/// <summary>
	/// Health checks grouped by application components.
	/// </summary>
	public List<HealthCheckComponent> Checks { get; set; } = [];
}
