using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UltimateMessengerSuggestions.Common.ExceptionHandlers;

namespace UltimateMessengerSuggestions.Common.Filters;

internal class FluentValidationFilter : IAsyncActionFilter
{
	private readonly IServiceProvider _serviceProvider;

	public FluentValidationFilter(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var failures = new List<ValidationFailure>();

		foreach (var arg in context.ActionArguments.Values)
		{
			if (arg == null) continue;

			var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());

			if (_serviceProvider.GetService(validatorType) is IValidator validator)
			{
				var validationContext = new ValidationContext<object>(arg);
				var result = await validator.ValidateAsync(validationContext);

				if (!result.IsValid)
					failures.AddRange(result.Errors);
			}
		}

		if (failures.Count > 0)
		{
			context.Result = new BadRequestObjectResult(CustomExceptionHandler.GenerateValidationProblemDetails(failures));
			return;
		}

		await next();
	}
}
