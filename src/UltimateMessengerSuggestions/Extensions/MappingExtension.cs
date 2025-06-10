using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Models.Dtos.Features;

namespace UltimateMessengerSuggestions.Extensions;

internal static class MappingExtension
{
	public static MediaFileDto ToDto(this MediaFile mediaFile)
	{
		MessageLocationDto? location = mediaFile switch
		{
			VkVoiceMediaFile voice => new MessageLocationDto(
				platform: Platform.Vk.ToString().ToLower(),
				dialogId: voice.VkConversation,
				messageId: voice.VkMessageId.ToString()
			),
			_ => null
		};

		return new MediaFileDto(
			description: mediaFile.Description,
			mediaUrl: mediaFile.MediaUrl,
			mediaType: mediaFile.MediaType.ToString().ToLower(),
			tags: mediaFile.Tags.Select(t => t.Name).ToList(),
			messageLocation: location);
	}

	public static IEnumerable<MediaFileDto> ToDto(this IEnumerable<MediaFile> mediaFiles)
	{
		return mediaFiles.Select(ToDto);
	}
}
