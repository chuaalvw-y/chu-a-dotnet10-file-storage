using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Models;

namespace ChuA.FileStorage.Services;

/// <summary>
/// Default application-facing file storage service.
/// </summary>
public sealed class FileStorageService : IFileStorageService
{
    private readonly IFileStorageProviderFactory providerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageService"/> class.
    /// </summary>
    /// <param name="providerFactory">The provider factory.</param>
    public FileStorageService(IFileStorageProviderFactory providerFactory)
    {
        this.providerFactory = providerFactory;
    }

    /// <inheritdoc />
    public Task<FileStorageSaveResult> SaveAsync(FileStorageSaveRequest request, CancellationToken cancellationToken = default)
        => providerFactory.GetProvider().SaveAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
        => providerFactory.GetProvider().OpenReadAsync(storageKey, cancellationToken);

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
        => providerFactory.GetProvider().ExistsAsync(storageKey, cancellationToken);

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        => providerFactory.GetProvider().DeleteAsync(storageKey, cancellationToken);

    /// <inheritdoc />
    public Task<FileStorageFileMetadata> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default)
        => providerFactory.GetProvider().GetMetadataAsync(storageKey, cancellationToken);

    /// <inheritdoc />
    public Task<Uri?> GetDownloadUriAsync(string storageKey, CancellationToken cancellationToken = default)
        => providerFactory.GetProvider().GetDownloadUriAsync(storageKey, cancellationToken);
}
