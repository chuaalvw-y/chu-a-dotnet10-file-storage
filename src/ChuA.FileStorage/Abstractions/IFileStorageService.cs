using ChuA.FileStorage.Models;

namespace ChuA.FileStorage.Abstractions;

/// <summary>
/// Provides application-facing operations for storing, reading, and deleting files.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file using the configured provider.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The result of the save operation.</returns>
    Task<FileStorageSaveResult> SaveAsync(FileStorageSaveRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a stored file for reading.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The opened file stream and metadata.</returns>
    Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a stored file exists.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the file exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a stored file when it exists.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when a file was deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads metadata for a stored file.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The file metadata.</returns>
    Task<FileStorageFileMetadata> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a download URI for a stored file when the provider supports URI generation.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A download URI, or <see langword="null"/> when unavailable.</returns>
    Task<Uri?> GetDownloadUriAsync(string storageKey, CancellationToken cancellationToken = default);
}
