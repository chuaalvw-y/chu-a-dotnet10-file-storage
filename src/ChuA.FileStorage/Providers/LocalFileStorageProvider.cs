using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Configuration;
using ChuA.FileStorage.Constants;
using ChuA.FileStorage.Models;
using ChuA.FileStorage.Utilities;
using Microsoft.Extensions.Options;

namespace ChuA.FileStorage.Providers;

/// <summary>
/// Stores files on the local file system.
/// </summary>
public sealed class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly IFileNameSanitizer fileNameSanitizer;
    private readonly IStorageKeyBuilder storageKeyBuilder;
    private readonly IOptions<FileStorageOptions> optionsAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileStorageProvider"/> class.
    /// </summary>
    /// <param name="fileNameSanitizer">The file name sanitizer.</param>
    /// <param name="storageKeyBuilder">The storage key builder.</param>
    /// <param name="optionsAccessor">The file storage options.</param>
    public LocalFileStorageProvider(
        IFileNameSanitizer fileNameSanitizer,
        IStorageKeyBuilder storageKeyBuilder,
        IOptions<FileStorageOptions> optionsAccessor)
    {
        this.fileNameSanitizer = fileNameSanitizer;
        this.storageKeyBuilder = storageKeyBuilder;
        this.optionsAccessor = optionsAccessor;
    }

    /// <inheritdoc />
    public string Name => FileStorageProviderNames.Local;

    /// <inheritdoc />
    public async Task<FileStorageSaveResult> SaveAsync(FileStorageSaveRequest request, CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.Value;
        FileStorageGuard.ValidateSaveRequest(request, options);

        var fileName = fileNameSanitizer.Sanitize(request.FileName);
        var storageKey = storageKeyBuilder.Build(request.Path, fileName);
        var fullPath = GetFullPath(storageKey, options.Local, ensureBaseDirectory: true);
        var overwrite = request.Overwrite ?? options.AllowOverwrite;

        if (!overwrite && File.Exists(fullPath))
        {
            throw new FileStorageException($"A file already exists for storage key '{storageKey}'.");
        }

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var target = new FileStream(
            fullPath,
            overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await request.Content.CopyToAsync(target, cancellationToken).ConfigureAwait(false);

        return new FileStorageSaveResult
        {
            ProviderName = Name,
            StorageKey = storageKey,
            FileName = fileName,
            ContentType = request.ContentType,
            Length = target.Length,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Uri = BuildPublicUri(storageKey, options.Local),
        };
    }

    /// <inheritdoc />
    public Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var fullPath = GetFullPath(normalizedKey, optionsAccessor.Value.Local, ensureBaseDirectory: false);
        if (!File.Exists(fullPath))
        {
            throw new FileStorageException($"No file exists for storage key '{normalizedKey}'.");
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
        var metadata = CreateMetadata(normalizedKey, fullPath);

        return Task.FromResult(new FileStorageReadResult
        {
            Content = stream,
            Metadata = metadata,
        });
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var fullPath = GetFullPath(normalizedKey, optionsAccessor.Value.Local, ensureBaseDirectory: false);
        return Task.FromResult(File.Exists(fullPath));
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var fullPath = GetFullPath(normalizedKey, optionsAccessor.Value.Local, ensureBaseDirectory: false);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<FileStorageFileMetadata> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var fullPath = GetFullPath(normalizedKey, optionsAccessor.Value.Local, ensureBaseDirectory: false);
        if (!File.Exists(fullPath))
        {
            throw new FileStorageException($"No file exists for storage key '{normalizedKey}'.");
        }

        return Task.FromResult(CreateMetadata(normalizedKey, fullPath));
    }

    /// <inheritdoc />
    public Task<Uri?> GetDownloadUriAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var uri = BuildPublicUri(normalizedKey, optionsAccessor.Value.Local);
        return Task.FromResult(uri);
    }

    private static FileStorageFileMetadata CreateMetadata(string storageKey, string fullPath)
    {
        var fileInfo = new FileInfo(fullPath);
        return new FileStorageFileMetadata
        {
            ProviderName = FileStorageProviderNames.Local,
            StorageKey = storageKey,
            FileName = Path.GetFileName(storageKey),
            Length = fileInfo.Length,
            LastModifiedUtc = fileInfo.LastWriteTimeUtc,
        };
    }

    private static string GetFullPath(string storageKey, LocalFileStorageOptions options, bool ensureBaseDirectory)
    {
        var basePath = Path.GetFullPath(options.BasePath);
        if (!Directory.Exists(basePath))
        {
            if (!options.CreateDirectoryIfMissing && ensureBaseDirectory)
            {
                throw new FileStorageException($"The configured local storage path '{basePath}' does not exist.");
            }

            if (ensureBaseDirectory)
            {
                Directory.CreateDirectory(basePath);
            }
        }

        var fullPath = Path.GetFullPath(Path.Combine(basePath, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        var basePathWithSeparator = Path.EndsInDirectorySeparator(basePath)
            ? basePath
            : basePath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(basePathWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new FileStorageException("The storage key resolves outside the configured local storage path.");
        }

        return fullPath;
    }

    private static Uri? BuildPublicUri(string storageKey, LocalFileStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PublicBaseUri))
        {
            return null;
        }

        var baseUri = new Uri(options.PublicBaseUri, UriKind.Absolute);
        return new Uri(baseUri, Uri.EscapeDataString(storageKey).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase));
    }
}
