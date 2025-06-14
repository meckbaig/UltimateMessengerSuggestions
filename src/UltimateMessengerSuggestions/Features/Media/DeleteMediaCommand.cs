using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Web;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.Common.Options;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Services;

namespace UltimateMessengerSuggestions.Features.Media;

/// <summary>
/// Command to delete a media file from the system.
/// </summary>
public record DeleteMediaCommand : IRequest<DeleteMediaResponse>
{
	/// <summary>
	/// The ID of the media file to be deleted.
	/// </summary>
	[FromRoute]
	public required int Id { get; init; }
}

/// <summary>
/// Response for the DeleteMediaCommand indicating the result of the deletion operation.
/// </summary>
public class DeleteMediaResponse
{
}

internal class DeleteMediaHandler : IRequestHandler<DeleteMediaCommand, DeleteMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaUploader _mediaUploader;
	private readonly WebDavOptions _webDavOptions;

	public DeleteMediaHandler(IAppDbContext context, IMediaUploader mediaUploader, IOptions<WebDavOptions> webDavOptions)
	{
		_context = context;
		_mediaUploader = mediaUploader;
		_webDavOptions = webDavOptions.Value;
	}

	public async Task<DeleteMediaResponse> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaFile = await _context.MediaFiles
			.Include(m => m.Tags)
			.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
		if (mediaFile == null)
		{
			throw new EntityNotFoundException($"Media file with Id {request.Id} not found.");
		}

		List<int> tagIdsToCheck = mediaFile.Tags
			.Select(t => t.Id)
			.ToList();

		List<Tag> tagsToRemove = _context.Tags
			.Where(t => tagIdsToCheck.Contains(t.Id) && t.MediaFiles.Single().Id == mediaFile.Id)
			.ToList();

		if (tagsToRemove.Count > 0)
			_context.Tags.RemoveRange(tagsToRemove);
		_context.MediaFiles.Remove(mediaFile);

		if (mediaFile.MediaUrl.Contains(_webDavOptions.PublicPreviewBase) 
			&& TryExtractFileName(mediaFile.MediaUrl, out string fileName))
		{
			await _mediaUploader.DeleteAsync(fileName, cancellationToken);
		}
		await _context.SaveChangesAsync(cancellationToken);

		return new DeleteMediaResponse();
	}

	public static bool TryExtractFileName(string previewUrl, out string fileName)
	{
		var uri = new Uri(previewUrl);
		var query = HttpUtility.ParseQueryString(uri.Query);
		var fileParam = query.Get("file");
		fileName = Path.GetFileName(fileParam);
		return fileName != null;
	}
}
