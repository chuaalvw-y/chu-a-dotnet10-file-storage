// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using ChuA.FileStorage.Models;

namespace ChuA.FileStorage.Abstractions;

/// <summary>
/// Defines the contract implemented by storage providers.
/// </summary>
public interface IFileStorageProvider
{
    /// <summary>
    /// Gets the unique provider name used in configuration.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Saves a file.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The save result.</returns>
    Task<FileStorageSaveResult> SaveAsync(FileStorageSaveRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The opened file stream and metadata.</returns>
    Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a file exists.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the file exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when a file was deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads file metadata.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The file metadata.</returns>
    Task<FileStorageFileMetadata> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a provider-specific download URI.
    /// </summary>
    /// <param name="storageKey">The normalized storage key.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A URI, or <see langword="null"/> when unavailable.</returns>
    Task<Uri?> GetDownloadUriAsync(string storageKey, CancellationToken cancellationToken = default);
}
