using AutoMapper;
using Meckbaig.Cqrs.Dto.Abstractions;
using Meckbaig.Cqrs.ListFliters.Attrubutes;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Dtos.Features.Suggestions;

namespace UltimateMessengerSuggestions.Models.Dtos.Features.Media;

/// <summary>
/// Information about a media file.
/// </summary>
public record MediaFileDto : IBaseDto
{
	/// <summary>
	/// Public identifier of the media file.
	/// </summary>
	[Filterable(CompareMethod.Equals)]
	public string Id { get; init; } = null!;

	/// <summary>
	/// Media file type.
	/// </summary>
	public string MediaType { get; init; } = null!;

	/// <summary>
	/// Media file URL.
	/// </summary>
	public string MediaUrl { get; init; } = null!;

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
	/// Constructor for creating a media file DTO with required fields and a list of tags.
	/// </summary>
	/// <param name="id">File identifier.</param>
	/// <param name="description">Media file description.</param>
	/// <param name="mediaUrl">Media file URL.</param>
	/// <param name="mediaType">Media file type.</param>
	/// <param name="tags">List of tags associated with the file.</param>
	/// <param name="messageLocation">Location of the message associated with the media file, if applicable.</param>
	public MediaFileDto(string id, string description, string mediaUrl, string mediaType, List<string> tags, MessageLocationDto? messageLocation = null)
	{
		Id = id;
		Description = description;
		MediaUrl = mediaUrl;
		MediaType = mediaType;
		Tags = tags;
		MessageLocation = messageLocation;
	}

	/// <inheritdoc />
	public static Type GetOriginType() => typeof(MediaFile);

	/// <remarks>
	/// For filters only
	/// </remarks>
	private class Mapping : Profile
	{
		public Mapping()
		{
			ShouldMapProperty = p => false;
			CreateMap<MediaFileDto, MediaFile>()
				.ForMember(d => d.Tags, o => o.MapFrom(s => s.Tags))
				.ForMember(d => d.PublicId, o => o.MapFrom(s => s.Id))
				.ForMember(d => d.Description, o => o.MapFrom(s => s.Description));
		}
	}
}
