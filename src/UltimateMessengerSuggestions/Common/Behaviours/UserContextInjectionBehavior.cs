using MediatR;
using UltimateMessengerSuggestions.Common.Abstractions;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.Services.Interfaces;

namespace UltimateMessengerSuggestions.Common.Behaviours;

internal class UserContextInjectionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IJwtProvider _jwtProvider;

	public UserContextInjectionBehavior(IHttpContextAccessor httpContextAccessor, IJwtProvider jwtProvider)
	{
		_httpContextAccessor = httpContextAccessor;
		_jwtProvider = jwtProvider;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var user = _httpContextAccessor.HttpContext?.User;

		if ((user?.Identity?.IsAuthenticated ?? false) == false)
			return await next(cancellationToken); 
		
		if (request is not IAuthentificatedRequest authRequest)
			return await next(cancellationToken);

		if (authRequest.IsAuthenticated)
			return await next(cancellationToken);

		var login = _jwtProvider.GetUserLoginFromClaimsPrincipal(user);

		var setUserLoginMethod = request.GetType().GetMethod(nameof(IAuthentificatedRequest<object>.SetUserLogin));
		setUserLoginMethod?.Invoke(request, [login]);

		return await next(cancellationToken);
	}
}
