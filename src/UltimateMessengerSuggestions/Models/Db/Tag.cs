using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;

namespace UltimateMessengerSuggestions.Models.Db;

/// <summary>
/// Tag used for searching media files.
/// </summary>
public class Tag : IEntityWithId
{
	/// <summary>
	/// Tag identifier.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Tag name.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Collection of media files associated with the tag.
	/// </summary>
	public ICollection<MediaFile> MediaFiles { get; set; } = [];
}
