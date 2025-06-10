using FluentValidation;
using MediatR;

namespace UltimateMessengerSuggestions.Common.Behaviours;

internal class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
	private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger;

	public UnhandledExceptionBehaviour(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
	{
		_logger = logger;
	}

	public async Task<TResponse> Handle
	(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		try
		{
			return await next(cancellationToken);
		}
		catch (ValidationException ex)
		{
			_logger.LogInformation("Validation error: {ErrorMessage}", ex.Message);
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unhandled exception");
			if (ex.InnerException != null)
				throw new Exception(ex.InnerException.Message, ex);
			throw;
		}
	}
}
