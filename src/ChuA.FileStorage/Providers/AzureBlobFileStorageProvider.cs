// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Configuration;
using ChuA.FileStorage.Constants;
using ChuA.FileStorage.Models;
using ChuA.FileStorage.Utilities;
using Microsoft.Extensions.Options;

namespace ChuA.FileStorage.Providers;

/// <summary>
/// Stores files in Azure Blob Storage.
/// </summary>
public sealed class AzureBlobFileStorageProvider : IFileStorageProvider
{
    private readonly IFileNameSanitizer fileNameSanitizer;
    private readonly IStorageKeyBuilder storageKeyBuilder;
    private readonly IOptions<FileStorageOptions> optionsAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobFileStorageProvider"/> class.
    /// </summary>
    /// <param name="fileNameSanitizer">The file name sanitizer.</param>
    /// <param name="storageKeyBuilder">The storage key builder.</param>
    /// <param name="optionsAccessor">The file storage options.</param>
    public AzureBlobFileStorageProvider(
        IFileNameSanitizer fileNameSanitizer,
        IStorageKeyBuilder storageKeyBuilder,
        IOptions<FileStorageOptions> optionsAccessor)
    {
        this.fileNameSanitizer = fileNameSanitizer;
        this.storageKeyBuilder = storageKeyBuilder;
        this.optionsAccessor = optionsAccessor;
    }

    /// <inheritdoc />
    public string Name => FileStorageProviderNames.AzureBlob;

    /// <inheritdoc />
    public async Task<FileStorageSaveResult> SaveAsync(FileStorageSaveRequest request, CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.Value;
        FileStorageGuard.ValidateSaveRequest(request, options);

        var fileName = fileNameSanitizer.Sanitize(request.FileName);
        var storageKey = storageKeyBuilder.Build(request.Path, fileName);
        var container = CreateContainerClient(options.AzureBlob);

        if (options.AzureBlob.CreateContainerIfMissing)
        {
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        var blob = container.GetBlobClient(storageKey);
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = request.ContentType },
            Metadata = request.Metadata.ToDictionary(StringComparer.OrdinalIgnoreCase),
            Conditions = (request.Overwrite ?? options.AllowOverwrite)
                ? null
                : new BlobRequestConditions { IfNoneMatch = ETag.All },
        };

        try
        {
            await blob.UploadAsync(request.Content, uploadOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException exception) when (exception.Status == 409)
        {
            throw new FileStorageException($"A blob already exists for storage key '{storageKey}'.", exception);
        }

        var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return new FileStorageSaveResult
        {
            ProviderName = Name,
            StorageKey = storageKey,
            FileName = fileName,
            ContentType = request.ContentType,
            Length = properties.Value.ContentLength,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Uri = blob.Uri,
        };
    }

    /// <inheritdoc />
    public async Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var blob = CreateContainerClient(optionsAccessor.Value.AzureBlob).GetBlobClient(normalizedKey);

        try
        {
            var download = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return new FileStorageReadResult
            {
                Content = download.Value.Content,
                Metadata = CreateMetadata(normalizedKey, download.Value.Details),
            };
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            throw new FileStorageException($"No blob exists for storage key '{normalizedKey}'.", exception);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var exists = await CreateContainerClient(optionsAccessor.Value.AzureBlob)
            .GetBlobClient(normalizedKey)
            .ExistsAsync(cancellationToken)
            .ConfigureAwait(false);

        return exists.Value;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var deleted = await CreateContainerClient(optionsAccessor.Value.AzureBlob)
            .GetBlobClient(normalizedKey)
            .DeleteIfExistsAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return deleted.Value;
    }

    /// <inheritdoc />
    public async Task<FileStorageFileMetadata> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var blob = CreateContainerClient(optionsAccessor.Value.AzureBlob).GetBlobClient(normalizedKey);

        try
        {
            var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return CreateMetadata(normalizedKey, properties.Value);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            throw new FileStorageException($"No blob exists for storage key '{normalizedKey}'.", exception);
        }
    }

    /// <inheritdoc />
    public Task<Uri?> GetDownloadUriAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var uri = CreateContainerClient(optionsAccessor.Value.AzureBlob).GetBlobClient(normalizedKey).Uri;
        return Task.FromResult<Uri?>(uri);
    }

    private static BlobContainerClient CreateContainerClient(AzureBlobFileStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ContainerName))
        {
            throw new FileStorageException("AzureBlob.ContainerName is required when Provider is AzureBlob.");
        }

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        }

        if (!string.IsNullOrWhiteSpace(options.ServiceUri))
        {
            var serviceUri = new Uri(options.ServiceUri.TrimEnd('/') + "/", UriKind.Absolute);
            return new BlobContainerClient(new Uri(serviceUri, options.ContainerName));
        }

        throw new FileStorageException("AzureBlob.ConnectionString or AzureBlob.ServiceUri is required when Provider is AzureBlob.");
    }

    private static FileStorageFileMetadata CreateMetadata(string storageKey, BlobProperties properties)
        => new()
        {
            ProviderName = FileStorageProviderNames.AzureBlob,
            StorageKey = storageKey,
            FileName = Path.GetFileName(storageKey),
            ContentType = properties.ContentType,
            Length = properties.ContentLength,
            LastModifiedUtc = properties.LastModified,
            Metadata = properties.Metadata.ToDictionary(StringComparer.OrdinalIgnoreCase),
        };

    private static FileStorageFileMetadata CreateMetadata(string storageKey, BlobDownloadDetails details)
        => new()
        {
            ProviderName = FileStorageProviderNames.AzureBlob,
            StorageKey = storageKey,
            FileName = Path.GetFileName(storageKey),
            ContentType = details.ContentType,
            Length = details.ContentLength,
            LastModifiedUtc = details.LastModified,
            Metadata = details.Metadata.ToDictionary(StringComparer.OrdinalIgnoreCase),
        };
}
