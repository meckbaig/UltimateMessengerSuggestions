using Meckbaig.Cqrs.Abstractons;
using Meckbaig.Cqrs.ListFliters.Abstractions;

namespace UltimateMessengerSuggestions.Common.Abstractions;

/// <summary>
/// Represents a base query for authenticated list operations, providing user login information and a response type.
/// </summary>
/// <typeparam name="TUserLoginDto">The type of the user login data transfer object. Must be a reference type.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the query. Must inherit from <see cref="BaseResponse"/>.</typeparam>
public abstract record BaseAuthentificatedListQuery<TUserLoginDto, TResponse> : BaseListQuery<TResponse>, IAuthentificatedRequest<TUserLoginDto>
	where TResponse : BaseResponse
	where TUserLoginDto : class
{
	/// <inheritdoc/>
	virtual public TUserLoginDto? UserLogin { get; private set; }

	/// <inheritdoc/>
	virtual public bool IsAuthenticated => UserLogin != null;

	/// <inheritdoc/>
	public void SetUserLogin(TUserLoginDto userLogin)
	{
		if (UserLogin != null)
			throw new InvalidOperationException("User login is already set.");
		UserLogin = userLogin;
	}
}
