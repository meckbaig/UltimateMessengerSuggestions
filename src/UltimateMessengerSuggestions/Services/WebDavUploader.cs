using Microsoft.Extensions.Options;
using System.Net;
using UltimateMessengerSuggestions.Common.Options;

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

	public async Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
	{
		var ext = Path.GetExtension(file.FileName);
		var hash = Guid.NewGuid().ToString("N")[..8];
		var remoteFileName = $"{hash}{ext}";
		var uploadUrl = new Uri($"{_settings.Endpoint}{remoteFileName}");

		using var client = new HttpClient(new HttpClientHandler
		{
			Credentials = new NetworkCredential(_settings.Username, _settings.Password)
		});

		Stream contentStream = ConvertIfNeededAsStream(file);

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

	private Stream ConvertIfNeededAsStream(IFormFile file)
	{
		/// TODO:
		return file.OpenReadStream();
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
