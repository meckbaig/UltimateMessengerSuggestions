using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.Extensions;

namespace UltimateMessengerSuggestions.Common.ExceptionHandlers;

internal class CustomExceptionHandler : IExceptionHandler
{
	private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;
	public CustomExceptionHandler()
	{
		_exceptionHandlers = new()
		{
			{ typeof(ValidationException), HandleValidationException },
			{ typeof(EntityNotFoundException), HandleNotFoundException },
		};
	}

	public async ValueTask<bool> TryHandleAsync
	(
		HttpContext httpContext,
		Exception ex,
		CancellationToken cancellationToken
	)
	{
		var exceptionType = ex.GetType();

		if (_exceptionHandlers.TryGetValue(exceptionType, out var value))
		{
			await value.Invoke(httpContext, ex);
			return true;
		}

		await HandleUnhandledException(httpContext, ex);
		return false;
	}

	private static async Task HandleUnhandledException(HttpContext context, Exception ex)
	{
		context.Response.StatusCode = StatusCodes.Status500InternalServerError;

		await context.Response.WriteAsJsonAsync(GenerateInternalProblemDetails(ex.Message));
	}

	private static async Task HandleValidationException(HttpContext context, Exception ex)
	{
		ValidationException validationException = (ex as ValidationException)!;
		context.Response.StatusCode = StatusCodes.Status400BadRequest;
		if (validationException.Errors.Count() > 0)
		{
			await context.Response.WriteAsJsonAsync
			(
				GenerateValidationProblemDetails(validationException.Errors)
			);
		}
		else
		{
			await context.Response.WriteAsJsonAsync(new ProblemDetails
			{
				Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
				Status = StatusCodes.Status400BadRequest,
				Title = "Validation error",
				Detail = validationException.Message
			});
		}

	}

	private static async Task HandleNotFoundException(HttpContext context, Exception ex)
	{
		context.Response.StatusCode = StatusCodes.Status404NotFound;

		await context.Response.WriteAsJsonAsync(new ProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-404-not-found",
			Status = StatusCodes.Status404NotFound,
			Title = "Resource not found",
			Detail = ex.Message
		});
	}

	#region public static members

	/// <summary>
	/// Creates an instance of <see cref="ValidationProblemDetails"/> with default parameters for a validation error.
	/// </summary>
	/// <param name="failures">A list of validation errors.</param>
	/// <returns>An instance of <see cref="ValidationProblemDetails"/>.</returns>
	public static ValidationProblemDetails GenerateValidationProblemDetails(IEnumerable<ValidationFailure> failures)
	{
		var errors = failures
			.GroupBy(e => e.PropertyName)
			.ToDictionary(
				g => string.Join('.', g.Key.Split('.').Select(JsonResponseExtensions.SerializerOptions.PropertyNamingPolicy!.ConvertName)),
				g => g.Select(e => e.ErrorMessage).ToArray());

		return new ValidationProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation error",
			Errors = errors
		};
	}

	public static ProblemDetails GenerateInternalProblemDetails(string detail)
	{
		return new ProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error",
			Status = StatusCodes.Status500InternalServerError,
			Title = "Unhandled exception occured",
			Detail = detail
		};
	}

	#endregion
}
