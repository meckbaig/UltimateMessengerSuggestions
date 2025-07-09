using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
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

	[HttpPost]
	public async Task<ActionResult<RegisterResponse>> Register(RegisterCommand command)
	{
		await _mediator.Send(command);
		return Ok();
	}

	[HttpGet]
	public async Task<ActionResult<GetLoginResponse>> Login(GetLoginQuery query)
	{
		await _mediator.Send(query);
		return Ok();
	}
}
