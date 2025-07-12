using System.Security.Claims;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Dtos.Auth;

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

	/// <summary>
	/// Extracts the user login information from the specified <see cref="ClaimsPrincipal"/>.
	/// </summary>
	/// <remarks>This method assumes that the <see cref="ClaimsPrincipal"/> contains the necessary claims to
	/// construct a  <see cref="UserLoginDto"/>. Ensure that the claims are properly populated before calling this
	/// method.</remarks>
	/// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> containing the claims from which the user login information is retrieved.</param>
	/// <returns>A <see cref="UserLoginDto"/> containing the user's login details, or <c>null</c> if the required claims are not
	/// present.</returns>
	UserLoginDto GetUserLoginFromClaimsPrincipal(ClaimsPrincipal claimsPrincipal);
}
