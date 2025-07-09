using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Features.Auth;

namespace UltimateMessengerSuggestions.Controllers.V1;

/// <summary>
/// Manages authentication operations such as login and registration.
/// </summary>
[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Manages authentication operations such as login and registration.")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController : Controller
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Constructor with parameters for dependency injection.
	/// </summary>
	public AuthController(IMediator mediator)
	{
		_mediator = mediator;
	}

	/// <summary>
	/// Registers a new user in the system for provided user hash.
	/// </summary>
	/// <returns>
	/// Bearer token for the user after successful registration.
	/// </returns>
	[HttpPost]
	public async Task<ActionResult<RegisterResponse>> Register(RegisterCommand command)
	{
		var result = await _mediator.Send(command);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Checks if a user exists in the system based on their messenger ID and client name.
	/// </summary>
	/// <returns>
	/// Bearer token for the user after successful registration.
	/// </returns>
	[HttpGet]
	public async Task<ActionResult<GetLoginResponse>> Login(GetLoginQuery query)
	{
		var result = await _mediator.Send(query);
		return result.ToJsonResponse();
	}
}
