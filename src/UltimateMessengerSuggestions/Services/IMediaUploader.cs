using UltimateMessengerSuggestions.Models.Db.Enums;

namespace UltimateMessengerSuggestions.Services;

/// <summary>
/// Interface for uploading files to a WebDAV server.
/// </summary>
public interface IMediaUploader
{
	/// <summary>
	/// Asynchronously uploads a file and returns the URL of the uploaded file.
	/// </summary>
	/// <remarks>The method uploads the provided file to a storage service and returns the URL where the file can be
	/// accessed. Ensure that the file meets any size or format requirements imposed by the storage service.</remarks>
	/// <param name="file">The file to be uploaded.</param>
	/// <param name="mediaType">Enumeration representing type of media file.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>URL of the uploaded file.</returns>
	Task<string> UploadAsync(IFormFile file, MediaType mediaType, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes the specified file asynchronously.
	/// </summary>
	/// <param name="fileName">The name of the file to delete.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>A task that represents the asynchronous delete operation.</returns>
	Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}
