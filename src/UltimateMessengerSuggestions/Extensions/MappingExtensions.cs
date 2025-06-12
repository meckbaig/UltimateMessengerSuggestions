using OpenTelemetry.Resources;
using FluentValidation;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Models.Dtos.Features.Media;
using UltimateMessengerSuggestions.Models.Dtos.Features.Suggestions;

namespace UltimateMessengerSuggestions.Extensions;

internal static class MappingExtensions
{
	public static MediaFileDto ToDto(this MediaFile source)
	{
		MessageLocationDto? location = source switch
		{
			VkVoiceMediaFile voice => new MessageLocationDto(
				platform: Platform.Vk.ToString().ToLower(),
				dialogId: voice.VkConversation,
				messageId: voice.VkMessageId.ToString()
			),
			_ => null
		};

		return new MediaFileDto(
			description: source.Description,
			mediaUrl: source.MediaUrl,
			mediaType: source.MediaType.ToString().ToLower(),
			tags: source.Tags.Select(t => t.Name).ToList(),
			messageLocation: location);
	}

	public static EditMediaFileDto ToEditDto(this MediaFile source)
	{
		MessageLocationDto? location = source switch
		{
			VkVoiceMediaFile voice => new MessageLocationDto(
				platform: Platform.Vk.ToString().ToLower(),
				dialogId: voice.VkConversation,
				messageId: voice.VkMessageId.ToString()
			),
			_ => null
		};

		return new EditMediaFileDto(
			id: source.Id,
			description: source.Description,
			mediaUrl: source.MediaUrl,
			mediaType: source.MediaType.ToString().ToLower(),
			tags: source.Tags.Select(t => t.Name).ToList(),
			messageLocation: location);
	}

	public static EditMediaFileDto ToEditDto(this MediaFileDto source, int id)
	{
		return new EditMediaFileDto(
			id: id,
			description: source.Description,
			mediaUrl: source.MediaUrl,
			mediaType: source.MediaType.ToString().ToLower(),
			tags: source.Tags,
			messageLocation: source.MessageLocation);
	}

	public static IEnumerable<MediaFileDto> ToDto(this IEnumerable<MediaFile> source)
	{
		return source.Select(ToDto);
	}

	public static async Task<MediaFile> ToDbModelAsync(
		this MediaFileDto source, 
		Func<IEnumerable<string>, CancellationToken, Task<IEnumerable<Tag>>> tagConversion,
		CancellationToken cancellationToken = default)
	{
		MediaFile mediaFile;

		if (source.MessageLocation?.Platform != null &&
			Enum.TryParse<Platform>(source.MessageLocation?.Platform, true, out var platform) &&
			platform == Platform.Vk)
		{
			mediaFile = new VkVoiceMediaFile
			{
				VkConversation = source.MessageLocation.DialogId,
				VkMessageId = long.Parse(source.MessageLocation.MessageId)
			};
		}
		else
		{
			mediaFile = new MediaFile();
		}

		mediaFile.Description = source.Description;
		mediaFile.MediaUrl = source.MediaUrl;

		if (Enum.TryParse<MediaType>(source.MediaType, true, out var mediaType))
		{
			mediaFile.MediaType = mediaType;
		}

		mediaFile.Tags = (await tagConversion.Invoke(source.Tags, cancellationToken)).ToArray();

		return mediaFile;
	}

	public static async Task<MediaFile> FromDto(
		this MediaFile source, 
		MediaFileDto dto, 
		Func<int, IEnumerable<Tag>, IEnumerable<string>, CancellationToken, Task<IEnumerable<Tag>>> tagConversion,
		CancellationToken cancellationToken = default)
	{
		if (Enum.TryParse<MediaType>(dto.MediaType, true, out var mediaType))
		{
			switch (mediaType)
			{
				case MediaType.Voice:
					if (source is not VkVoiceMediaFile vkVoiceMediaFile)
					{
						throw new InvalidOperationException("Source media file must be of type VkVoiceMediaFile for voice media type.");
					}
					if (dto.MessageLocation == null)
					{
						throw new ValidationException([new(nameof(dto.MessageLocation), "MessageLocation is required for voice media type.")]);
					}
					if (!Enum.TryParse<Platform>(dto.MessageLocation?.Platform, true, out var platform) || platform != Platform.Vk)
					{
						throw new ValidationException([new($"{nameof(dto.MessageLocation)}.{nameof(dto.MessageLocation.Platform)}", "Platform can not be changed")]);
					}
					vkVoiceMediaFile.VkConversation = dto.MessageLocation.DialogId;
					vkVoiceMediaFile.VkMessageId = long.Parse(dto.MessageLocation.MessageId);
					break;
				default:
					break;
			}
		}
		source.MediaUrl = dto.MediaUrl;
		source.Tags = (await tagConversion.Invoke(source.Id, source.Tags, dto.Tags, cancellationToken)).ToList();
		source.Description = dto.Description;
		return source;
	}
}
