using UltimateMessengerSuggestions.Models.Db;

namespace UltimateMessengerSuggestions.Services.Interfaces;

/// <summary>
/// Provides methods for generating JSON Web Tokens (JWT) for user authentication.
/// </summary>
public interface IJwtProvider
{
	/// <summary>
	/// Generates JWT for provided account.
	/// </summary>
	/// <param name="account">Messenger account for a user in the system.</param>
	/// <returns>Json web token.</returns>
	string GenerateToken(MessengerAccount account);
}
