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

	/// <summary>
	/// Edits an existing media item based on the provided command.
	/// </summary>
	/// <remarks>This method processes the edit operation by sending the provided command to the mediator. The
	/// response is returned in JSON format.</remarks>
	/// <param name="command">The command containing the details of the media item to be edited, including its identifier and updated properties.</param>
	/// <returns>An <see cref="ActionResult{T}"/> containing the updated <see cref="EditMediaCommand"/> object.</returns>
	[HttpPost("{id}")]
	public async Task<IActionResult> EditMedia(EditMediaCommand command)
	{
		var result = await _mediator.Send(command);
		return Ok();
	}

	/// <summary>
	/// Deletes a media item based on the specified command.
	/// </summary>
	/// <remarks>This method uses the mediator pattern to process the delete operation. Ensure that the <paramref
	/// name="command"/> contains valid data to avoid errors.</remarks>
	/// <param name="command">The command containing the details of the media item to delete.  This must include the necessary identifiers and
	/// any required parameters.</param>
	/// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="DeleteMediaResponse"/> that indicates the result of the
	/// delete operation.</returns>
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteMedia(DeleteMediaCommand command)
	{
		await _mediator.Send(command);
		return Ok();
	}

	/// <summary>
	/// Retrieves a list of media items based on the specified query parameters.
	/// </summary>
	/// <remarks>This method processes the provided query using the mediator pattern to retrieve the requested media
	/// items. The response is returned in JSON format.</remarks>
	/// <param name="query">The query parameters used to filter and retrieve the media items. Cannot be null.</param>
	/// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="GetMediaResponse"/> object with the list of media items.
	/// If no items match the query, the response will indicate an empty result.</returns>
	[HttpGet]
	public async Task<ActionResult<GetMediaResponse>> GetList(GetMediaQuery query)
	{
		var result = await _mediator.Send(query);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Edits an existing media item based on the provided command.
	/// </summary>
	/// <remarks>This method processes the edit operation by sending the provided command to the mediator. The
	/// response is returned in JSON format.</remarks>
	/// <param name="command">The command containing the details of the media item to be edited, including its identifier and updated properties.</param>
	/// <returns>An <see cref="ActionResult{T}"/> containing the updated <see cref="EditMediaCommand"/> object.</returns>
	[HttpPost("upload")]
	public async Task<ActionResult<UploadMediaResponse>> UploadMedia(UploadMediaCommand command)
	{
		var result = await _mediator.Send(command);
		return result.ToJsonResponse();
	}
}
