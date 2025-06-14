using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Net;
using UltimateMessengerSuggestions.Common.Options;
using UltimateMessengerSuggestions.Models.Db.Enums;

namespace UltimateMessengerSuggestions.Services;

internal class WebDavUploader : IMediaUploader
{
	private readonly ILogger<WebDavUploader> _logger;
	private readonly WebDavOptions _settings;

	public WebDavUploader(IOptions<WebDavOptions> options, ILogger<WebDavUploader> logger)
	{
		_settings = options.Value;
		_logger = logger;
	}

	public async Task<string> UploadAsync(IFormFile file, MediaType mediaType, CancellationToken cancellationToken = default)
	{
		var hash = Guid.NewGuid().ToString("N")[..8];

		using var client = new HttpClient(new HttpClientHandler
		{
			Credentials = new NetworkCredential(_settings.Username, _settings.Password)
		});

		(Stream contentStream, string extension) = await ConvertIfNeededAsStreamAsync(file, mediaType);

		var remoteFileName = $"{hash}{extension}";
		var uploadUrl = new Uri($"{_settings.Endpoint}{remoteFileName}");

		using var content = new StreamContent(contentStream);
		var response = await client.PutAsync(uploadUrl, content, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			string errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogError("Failed to upload file to WebDAV server. Reason: {ReasonPhrase}. Content: {Content}",
				response.ReasonPhrase, errorMessage);
			throw new Exception($"Upload failed: {response.StatusCode}");
		}
		_logger.LogInformation("File '{FileName}' successfully uploaded.", remoteFileName);

		return $"{_settings.PublicPreviewBase}{Uri.EscapeDataString(remoteFileName)}";
	}

	private async Task<(Stream Stream, string Extension)> ConvertIfNeededAsStreamAsync(IFormFile file, MediaType mediaType)
	{
		switch (mediaType)
		{
			case MediaType.Picture:
				return await ConvertPictureIfNeededAsStreamAsync(file);
			default:
				throw new NotSupportedException($"Media type '{mediaType}' is not supported for upload.");
		}
	}

	private async Task<(Stream Stream, string Extension)> ConvertPictureIfNeededAsStreamAsync(IFormFile file)
	{
		try
		{
			string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (ext is ".jpg" or ".jpeg" or ".png" or ".webp")
				return (file.OpenReadStream(), ext);

			using var image = await Image.LoadAsync(file.OpenReadStream());
			var memoryStream = new MemoryStream();
			await image.SaveAsJpegAsync(memoryStream, new JpegEncoder
			{
				Quality = 90
			});
			memoryStream.Seek(0, SeekOrigin.Begin);
			return (memoryStream, ".jpg");
		}
		catch (SixLabors.ImageSharp.UnknownImageFormatException)
		{
			throw new InvalidOperationException("Image format is not supported.");
		}
		catch (SixLabors.ImageSharp.ImageFormatException)
		{
			throw new InvalidOperationException("File is not a valid image.");
		}
	}

	public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
	{
		var deleteUrl = new Uri($"{_settings.Endpoint}{fileName}");
		using var client = new HttpClient(new HttpClientHandler
		{
			Credentials = new NetworkCredential(_settings.Username, _settings.Password)
		});

		var response = await client.DeleteAsync(deleteUrl, cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			string errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogError("Failed to delete file from WebDAV server. Reason: {ReasonPhrase}. Content: {Content}",
				response.ReasonPhrase, errorMessage);
			throw new Exception($"Delete failed: {response.ReasonPhrase}");
		}
		_logger.LogInformation("File '{FileName}' successfully deleted.", fileName);
	}
}
