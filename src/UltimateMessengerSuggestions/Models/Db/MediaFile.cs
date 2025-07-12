using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;
using UltimateMessengerSuggestions.Models.Db.Enums;

namespace UltimateMessengerSuggestions.Models.Db;

/// <summary>
/// A media file that can be associated with tags.
/// </summary>
public class MediaFile : IEntityWithId, IEntityWithPublicId
{
	/// <summary>
	/// File identifier.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Identifier exposed to user.
	/// </summary>
	public string PublicId { get; set; } = null!;

	/// <summary>
	/// Identifier of the user who owns the media file.
	/// </summary>
	public int OwnerId { get; set; }

	/// <summary>
	/// Indicates whether the media file is free to use.
	/// </summary>
	public bool IsPublic { get; set; } = false;

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

	/// <summary>
	/// User who created the media file.
	/// </summary>
	public User Owner { get; set; } = null!;
}
