namespace UltimateMessengerSuggestions.Common.Abstractions;

/// <inheritdoc/>
/// <typeparam name="TUserLoginDto"></typeparam>
public interface IAuthentificatedRequest<TUserLoginDto> : IAuthentificatedRequest where TUserLoginDto : class
{
	/// <summary>
	/// The user login information associated with the query.
	/// </summary>
	public TUserLoginDto? UserLogin { get; }

	/// <summary>
	/// Sets the user login information for the query.
	/// </summary>
	/// <param name="userLogin">The user login information associated with the query.</param>
	void SetUserLogin(TUserLoginDto userLogin);
}

/// <summary>
/// Represents a request that requires or may contain user authentication.
/// </summary>
/// <remarks>
/// Should not be used directly, use <see cref="IAuthentificatedRequest{TUserLoginDto}"/> instead.
/// </remarks>
public interface IAuthentificatedRequest
{
	/// <summary>
	/// Indicates whether the user is authenticated based on the presence of user login information.
	/// </summary>
	bool IsAuthenticated { get; }
}
