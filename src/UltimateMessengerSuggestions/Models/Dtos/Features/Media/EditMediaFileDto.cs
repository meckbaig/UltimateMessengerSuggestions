using UltimateMessengerSuggestions.Models.Dtos.Features.Suggestions;

namespace UltimateMessengerSuggestions.Models.Dtos.Features.Media;

/// <summary>
/// Represents a data transfer object for editing a media file.
/// </summary>
public record EditMediaFileDto : MediaFileDto
{
	/// <summary>
	/// File identifier.
	/// </summary>
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
}
