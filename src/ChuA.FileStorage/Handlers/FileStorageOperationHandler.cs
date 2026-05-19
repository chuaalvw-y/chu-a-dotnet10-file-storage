using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Models;

namespace ChuA.FileStorage.Handlers;

/// <summary>
/// Base class for application-specific file operation handlers.
/// </summary>
public abstract class FileStorageOperationHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageOperationHandler"/> class.
    /// </summary>
    /// <param name="fileStorageService">The file storage service.</param>
    protected FileStorageOperationHandler(IFileStorageService fileStorageService)
    {
        FileStorageService = fileStorageService;
    }

    /// <summary>
    /// Gets the file storage service available to derived handlers.
    /// </summary>
    protected IFileStorageService FileStorageService { get; }

    /// <summary>
    /// Saves a file after the handler has performed any application-specific checks.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The save result.</returns>
    protected Task<FileStorageSaveResult> SaveFileAsync(
        FileStorageSaveRequest request,
        CancellationToken cancellationToken = default)
        => FileStorageService.SaveAsync(request, cancellationToken);
}
