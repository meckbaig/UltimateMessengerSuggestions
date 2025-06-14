using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UltimateMessengerSuggestions.Services;

namespace UltimateMessengerSuggestions.Features.Media;

/// <summary>
/// Command to upload a media file to the system.
/// </summary>
public record UploadMediaCommand : IRequest<UploadMediaResponse>
{
	/// <summary>
	/// The media file to be uploaded.
	/// </summary>
	[FromForm]
	public IFormFile File { get; init; } = null!;
}

/// <summary>
/// Response for the UploadMediaCommand containing the URL of the uploaded media file.
/// </summary>
public class UploadMediaResponse
{
	/// <summary>
	/// The URL where the uploaded media file can be previewed.
	/// </summary>
	public required string PreviewUrl { get; init; }
}

internal class UploadMediaHandler : IRequestHandler<UploadMediaCommand, UploadMediaResponse>
{
	private readonly IMediaUploader _mediaUploader;

	public UploadMediaHandler(IMediaUploader mediaUploader)
	{
		_mediaUploader = mediaUploader;
	}

	public async Task<UploadMediaResponse> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
	{
		if (request.File == null || request.File.Length == 0)
		{
			throw new ValidationException([new(nameof(request.File), "File cannot be null or empty.")]);
		}
		return new UploadMediaResponse
		{
			PreviewUrl = await _mediaUploader.UploadAsync(request.File, cancellationToken)
		};
	}
}
