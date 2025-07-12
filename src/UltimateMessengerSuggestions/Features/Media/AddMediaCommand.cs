using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Dtos.Features.Media;
using static UltimateMessengerSuggestions.Features.Media.AddMediaCommand;

namespace UltimateMessengerSuggestions.Features.Media;

/// <summary>
/// Command to add a media file to the system.
/// </summary>
public record AddMediaCommand : IRequest<AddMediaResponse>
{
	/// <summary>
	/// The body of the request containing the media file information.
	/// </summary>
	[FromBody]
	public required BodyParameters Body { get; init; }

	/// <summary>
	/// Parameters for the body of the request to add a media file.
	/// </summary>
	public record BodyParameters
	{
		/// <summary>
		/// The media file to be added.
		/// </summary>
		public required EditMediaFileDto MediaFile { get; init; }
	}
}

/// <summary>
/// Response for the AddMediaCommand containing the added media file information.
/// </summary>
public class AddMediaResponse
{
	/// <summary>
	/// The media file that was added to the system.
	/// </summary>
	public required MediaFileDto MediaFile { get; init; }
}

internal class AddMediaValidator : AbstractValidator<AddMediaCommand>
{
	public AddMediaValidator()
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
				.SetValidator(new EditMediaFileDto.Validator());
		}
	}
}

internal class AddMediaHandler : IRequestHandler<AddMediaCommand, AddMediaResponse>
{
	private readonly IAppDbContext _context;

	public AddMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<AddMediaResponse> Handle(AddMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaFile = await request.Body.MediaFile.ToDbModelAsync(TagConversion, cancellationToken);
		await _context.MediaFiles.AddAsync(mediaFile, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		return new AddMediaResponse
		{
			MediaFile = mediaFile.ToDto()
		};
	}

	private async Task<IEnumerable<Tag>> TagConversion(IEnumerable<string> tags, CancellationToken cancellationToken)
	{
		var normalizedTags = tags
			.Select(t => t.Trim().ToLowerInvariant())
			.ToHashSet();

		List<Tag> existingTags = await _context.Tags
			.Where(t => normalizedTags.Contains(t.Name))
			.ToListAsync(cancellationToken);

		List<Tag> tagsToCreate = normalizedTags
			.Except(existingTags.Select(t => t.Name))
			.Select(name => new Tag { Name = name })
			.ToList();

		return tagsToCreate.Concat(existingTags);
	}
}
