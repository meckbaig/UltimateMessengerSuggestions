using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Dtos.Features.Suggestions;
using static UltimateMessengerSuggestions.Features.Media.EditMediaCommand;

namespace UltimateMessengerSuggestions.Features.Media;

/// <summary>
/// Command to edit a media file.
/// </summary>
public record EditMediaCommand : IRequest<EditMediaResponse>
{
	/// <summary>
	/// The ID of the media file to be edited.
	/// </summary>
	[FromRoute]
	public required int Id { get; init; }

	/// <summary>
	/// The body of the request containing the media file information.
	/// </summary>
	[FromBody]
	public required BodyParameters Body { get; init; }

	/// <summary>
	/// Parameters for the body of the request to edit a media file.
	/// </summary>
	public record BodyParameters
	{
		/// <summary>
		/// The media file to be edited.
		/// </summary>
		public required MediaFileDto MediaFile { get; init; }
	}
}

/// <summary>
/// Response for the EditMediaCommand indicating the result of the deletion operation.
/// </summary>
public class EditMediaResponse
{
}

internal class EditMediaValidator : AbstractValidator<EditMediaCommand>
{
	public EditMediaValidator()
	{
		RuleFor(x => x.Body)
			.NotNull()
			.SetValidator(new BodyParametersValidator());
	}

	internal class BodyParametersValidator : AbstractValidator<BodyParameters>
	{
		public BodyParametersValidator()
		{
			RuleFor(x => x.MediaFile)
				.NotNull()
				.SetValidator(new MediaFileDto.Validator());
		}
	}
}

internal class EditMediaHandler : IRequestHandler<EditMediaCommand, EditMediaResponse>
{
	private readonly IAppDbContext _context;

	public EditMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<EditMediaResponse> Handle(EditMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaFile = await _context.MediaFiles
			.Include(m => m.Tags)
			.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
		if (mediaFile == null)
		{
			throw new EntityNotFoundException($"Media file with Id {request.Id} not found.");
		}

		mediaFile = await mediaFile.FromDto(request.Body.MediaFile, TagConversion, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		return new EditMediaResponse();
	}

	private async Task<IEnumerable<Tag>> TagConversion(
		int currentMediaFileId,
		IEnumerable<Tag> currentMediaFileTags,
		IEnumerable<string> newTags, 
		CancellationToken cancellationToken)
	{
		var normalizedTags = newTags
			.Select(t => t.Trim().ToLowerInvariant())
			.ToHashSet();

		List<Tag> existingTags = await _context.Tags
			.Where(t => normalizedTags.Contains(t.Name))
			.ToListAsync(cancellationToken);

		List<Tag> tagsToCreate = normalizedTags
			.Except(existingTags.Select(t => t.Name))
			.Select(name => new Tag { Name = name })
			.ToList();

		List<int> tagIdsToCheck = currentMediaFileTags
			.Where(t => !normalizedTags.Contains(t.Name))
			.Select(t => t.Id)
			.ToList();

		List<Tag> tagsToRemove = _context.Tags
			.Where(t => tagIdsToCheck.Contains(t.Id) && t.MediaFiles.Single().Id == currentMediaFileId)
			.ToList();

		if (tagsToRemove.Count > 0)
			_context.Tags.RemoveRange(tagsToRemove);

		return tagsToCreate.Concat(existingTags);
	}
}
