using AutoMapper;
using FluentValidation;
using Meckbaig.Cqrs.Dto.Abstractions;
using Meckbaig.Cqrs.ListFliters.Attrubutes;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Models.Dtos.Features.Suggestions;

namespace UltimateMessengerSuggestions.Models.Dtos.Features.Media;

/// <summary>
/// Information about an editable media file.
/// </summary>
public record EditMediaFileDto : IEditDto
{
	/// <summary>
	/// Media file type.
	/// </summary>
	public string MediaType { get; init; } = null!;

    /// <summary>
    /// Media file URL.
    /// </summary>
    public string MediaUrl { get; init; } = null!;

	/// <summary>
	/// Indicates whether the media file is free to use.
	/// </summary>
	public bool IsPublic { get; init; } = false;

	/// <summary>
	/// Description of the media file content.
	/// </summary>
	[Filterable(CompareMethod.OriginalContainsInput)]
	public string Description { get; init; } = null!;

	/// <summary>
	/// Location of the message associated with the media file, if applicable.
	/// </summary>
	public MessageLocationDto? MessageLocation { get; init; }

	/// <summary>
	/// List of tags associated with the file.
	/// </summary>
	[Filterable(CompareMethod.Custom)]
	public List<string> Tags { get; init; } = [];

	/// <summary>
	/// Constructor for creating an editable media file DTO with required fields and a list of tags.
	/// </summary>
	/// <param name="description">Media file description.</param>
	/// <param name="mediaUrl">Media file URL.</param>
	/// <param name="mediaType">Media file type.</param>
	/// <param name="tags">List of tags associated with the file.</param>
	/// <param name="messageLocation">Location of the message associated with the media file, if applicable.</param>
	public EditMediaFileDto(string description, string mediaUrl, string mediaType, List<string> tags, MessageLocationDto? messageLocation = null)
    {
        Description = description;
        MediaUrl = mediaUrl;
        MediaType = mediaType;
        Tags = tags;
        MessageLocation = messageLocation;
	}

	/// <inheritdoc />
	public static Type GetValidatorType() => typeof(Validator);

	/// <inheritdoc />
	public static Type GetOriginType() => typeof(MediaFile);

	internal class Validator : AbstractValidator<EditMediaFileDto>
	{
		public Validator()
		{
			RuleFor(x => x.MediaType).MustBeValidEnum<EditMediaFileDto, MediaType>();
			RuleFor(x => x.MediaUrl).NotEmpty();
			RuleFor(x => x.IsPublic).NotNull();
			RuleFor(x => x.Description).NotEmpty();
			RuleFor(x => x.Tags)
				.NotNull()
				.NotEmpty()
				.Must(tags => tags.All(tag => !string.IsNullOrWhiteSpace(tag)))
				.WithMessage("Tags cannot be empty or whitespace.");
			RuleFor(x => x.MessageLocation)
				.NotNull()
				.WithMessage("MessageLocation is required when MediaType is 'voice'.")
				.When(x => Enum.TryParse<MediaType>(x.MediaType, true, out var type) && type == Db.Enums.MediaType.Voice);
			RuleFor(x => x.MessageLocation)
				.SetValidator(new MessageLocationDto.Validator())
				.When(x => x.MessageLocation != null);
		}
	}
}
