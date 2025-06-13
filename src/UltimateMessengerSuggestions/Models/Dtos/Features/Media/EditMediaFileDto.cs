using AutoMapper;
using Meckbaig.Cqrs.Dto.Abstractions;
using Meckbaig.Cqrs.ListFliters.Attrubutes;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Dtos.Features.Suggestions;

namespace UltimateMessengerSuggestions.Models.Dtos.Features.Media;

/// <summary>
/// Represents a data transfer object for editing a media file.
/// </summary>
public record EditMediaFileDto : MediaFileDto, IEditDto
{
	/// <summary>
	/// File identifier.
	/// </summary>
	[Filterable(CompareMethod.Equals)]
	public int Id { get; init; }

	/// <summary>
	/// Constructor for creating a media file DTO with required fields and a list of tags.
	/// </summary>
	/// <param name="id">File identifier.</param>
	/// <param name="description">Media file description.</param>
	/// <param name="mediaUrl">Media file URL.</param>
	/// <param name="mediaType">Media file type.</param>
	/// <param name="tags">List of tags associated with the file.</param>
	/// <param name="messageLocation">Location of the message associated with the media file, if applicable.</param>
	public EditMediaFileDto(int id, string description, string mediaUrl, string mediaType, List<string> tags, MessageLocationDto? messageLocation = null)
		: base(description, mediaUrl, mediaType, tags, messageLocation)
	{
		Id = id;
	}

	/// <inheritdoc />
	public static Type GetValidatorType() => typeof(Validator);

	/// <remarks>
	/// For filters only
	/// </remarks>
	private class Mapping : Profile
	{
		public Mapping()
		{
			CreateMap<EditMediaFileDto, MediaFile>()
				.ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
				.ForMember(d => d.Description, o => o.MapFrom(s => s.Description));
		}
	}
}
