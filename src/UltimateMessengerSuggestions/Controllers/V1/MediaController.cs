using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Features.Media;

namespace UltimateMessengerSuggestions.Controllers.V1;

/// <summary>
/// Manages media parameters.
/// </summary>
[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Manages media parameters.")]
[Produces(MediaTypeNames.Application.Json)]
public class MediaController : ControllerBase
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public MediaController(IMediator mediator)
	{
		_mediator = mediator;
	}

	/// <summary>
	/// Adds a new media item to the system.
	/// </summary>
	/// <remarks>This method processes the <paramref name="command"/> using the mediator pattern to handle the  add
	/// media operation. The response is returned in JSON format.</remarks>
	/// <param name="command">The command containing the details of the media item to be added.  This parameter cannot be null.</param>
	/// <returns>An <see cref="ActionResult{T}"/> containing the response for the add media operation. The response includes details
	/// about the success or failure of the operation.</returns>
	[HttpPut]
	public async Task<ActionResult<AddMediaResponse>> AddMedia(AddMediaCommand command)
	{
		var result = await _mediator.Send(command);
		return result.ToJsonResponse();
	}


}
