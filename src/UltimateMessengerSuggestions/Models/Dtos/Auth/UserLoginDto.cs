using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UltimateMessengerSuggestions.Models.Dtos.Auth;

/// <summary>
/// User login data from Bearer Authorization.
/// </summary>
public record UserLoginDto
{
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
	/// <param name="messengerId">Unique identifier for the user from external messenger.</param>
	/// <param name="client">Client name from which the user is registered.</param>
	public UserLoginDto(string messengerId, string client)
	{
		MessengerId = messengerId;
		Client = client;
	}
}
