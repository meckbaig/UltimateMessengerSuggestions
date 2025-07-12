namespace UltimateMessengerSuggestions.Models.Dtos.Auth;

/// <summary>
/// User login data from Bearer Authorization.
/// </summary>
public record UserLoginDto
{
	/// <summary>
	/// Unique identifier for the user in the system.
	/// </summary>
	public int UserId { get; }

	/// <summary>
	/// Unique identifier for the user from external messenger.
	/// </summary>
	public string MessengerId { get; }

	/// <summary>
	/// Client name from which the user is registered.
	/// </summary>
	public string Client { get; }

	/// <summary>
	/// Creates a new instance of <see cref="UserLoginDto"/>.
	/// </summary>
	/// <param name="userId">Unique identifier for the user in the system.</param>
	/// <param name="messengerId">Unique identifier for the user from external messenger.</param>
	/// <param name="client">Client name from which the user is registered.</param>
	public UserLoginDto(int userId, string messengerId, string client)
	{
		UserId = userId;
		MessengerId = messengerId;
		Client = client;
	}
}
