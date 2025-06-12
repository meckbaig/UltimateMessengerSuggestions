using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Features;

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
	/// Gets media suggestions based on the provided search string and client identifier.
	/// </summary>
	/// <param name="query">Search media by search string query parameters.</param>
	/// <returns>Response results with media files that match the search criteria.</returns>
	[HttpGet]
	public async Task<ActionResult<GetMediaResponse>> GetSuggestions(GetMediaQuery query)
	{
		var result = await _mediator.Send(query);
		return result.ToJsonResponse();
	}


}
