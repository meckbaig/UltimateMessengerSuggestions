namespace UltimateMessengerSuggestions.Models.Db;

/// <summary>
/// Represents a voice media file associated with a VK conversation and message.
/// </summary>
public class VkVoiceMediaFile : MediaFile
{
	/// <summary>
	/// Unique identifier of the VK conversation this media file belongs to.
	/// </summary>
	public required string VkConversation { get; set; }

	/// <summary>
	/// Unique identifier of the VK message this media file is associated with.
	/// </summary>
	public required long VkMessageId { get; set; }
}
