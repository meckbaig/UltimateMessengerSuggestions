using UltimateMessengerSuggestions.Models.Db.Enums;

namespace UltimateMessengerSuggestions.Models.Db;

/// <summary>
/// A media file that can be associated with tags.
/// </summary>
public class MediaFile
{
	/// <summary>
	/// File identifier.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Media file type.
	/// </summary>
	public MediaType MediaType { get; set; }

	/// <summary>
	/// Media file URL.
	/// </summary>
	public string MediaUrl { get; set; } = null!;

	/// <summary>
	/// Description of the media file content.
	/// </summary>
	public string Description { get; set; } = null!;

	/// <summary>
	/// List of tags associated with the file.
	/// </summary>
	public ICollection<Tag> Tags { get; set; } = [];
}
