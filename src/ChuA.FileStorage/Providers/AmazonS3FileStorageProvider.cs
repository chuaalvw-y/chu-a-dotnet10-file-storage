// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Configuration;
using ChuA.FileStorage.Constants;
using ChuA.FileStorage.Models;
using ChuA.FileStorage.Utilities;
using Microsoft.Extensions.Options;

namespace ChuA.FileStorage.Providers;

/// <summary>
/// Stores files in Amazon S3 or S3-compatible storage.
/// </summary>
public sealed class AmazonS3FileStorageProvider : IFileStorageProvider
{
    private readonly IFileNameSanitizer fileNameSanitizer;
    private readonly IStorageKeyBuilder storageKeyBuilder;
    private readonly IOptions<FileStorageOptions> optionsAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonS3FileStorageProvider"/> class.
    /// </summary>
    /// <param name="fileNameSanitizer">The file name sanitizer.</param>
    /// <param name="storageKeyBuilder">The storage key builder.</param>
    /// <param name="optionsAccessor">The file storage options.</param>
    public AmazonS3FileStorageProvider(
        IFileNameSanitizer fileNameSanitizer,
        IStorageKeyBuilder storageKeyBuilder,
        IOptions<FileStorageOptions> optionsAccessor)
    {
        this.fileNameSanitizer = fileNameSanitizer;
        this.storageKeyBuilder = storageKeyBuilder;
        this.optionsAccessor = optionsAccessor;
    }

    /// <inheritdoc />
    public string Name => FileStorageProviderNames.AmazonS3;

    /// <inheritdoc />
    public async Task<FileStorageSaveResult> SaveAsync(FileStorageSaveRequest request, CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.Value;
        FileStorageGuard.ValidateSaveRequest(request, options);
        EnsureBucketConfigured(options.AmazonS3);

        var fileName = fileNameSanitizer.Sanitize(request.FileName);
        var storageKey = storageKeyBuilder.Build(request.Path, fileName);
        using var client = CreateClient(options.AmazonS3);

        if (!(request.Overwrite ?? options.AllowOverwrite) && await ObjectExistsAsync(client, options.AmazonS3.BucketName!, storageKey, cancellationToken).ConfigureAwait(false))
        {
            throw new FileStorageException($"An S3 object already exists for storage key '{storageKey}'.");
        }

        var putRequest = new PutObjectRequest
        {
            BucketName = options.AmazonS3.BucketName,
            Key = storageKey,
            InputStream = request.Content,
            ContentType = request.ContentType,
            AutoCloseStream = false,
        };

        foreach (var item in request.Metadata)
        {
            putRequest.Metadata[item.Key] = item.Value;
        }

        await client.PutObjectAsync(putRequest, cancellationToken).ConfigureAwait(false);

        var metadata = await client.GetObjectMetadataAsync(options.AmazonS3.BucketName, storageKey, cancellationToken).ConfigureAwait(false);
        return new FileStorageSaveResult
        {
            ProviderName = Name,
            StorageKey = storageKey,
            FileName = fileName,
            ContentType = request.ContentType,
            Length = metadata.ContentLength,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Uri = BuildObjectUri(options.AmazonS3, storageKey),
        };
    }

    /// <inheritdoc />
    public async Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.Value.AmazonS3;
        EnsureBucketConfigured(options);

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        var client = CreateClient(options);

        try
        {
            var response = await client.GetObjectAsync(options.BucketName, normalizedKey, cancellationToken).ConfigureAwait(false);
            return new FileStorageReadResult
            {
                Content = new OwnedStream(response.ResponseStream, response, client),
                Metadata = CreateMetadata(normalizedKey, response),
            };
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            client.Dispose();
            throw new FileStorageException($"No S3 object exists for storage key '{normalizedKey}'.", exception);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.Value.AmazonS3;
        EnsureBucketConfigured(options);

        using var client = CreateClient(options);
        return await ObjectExistsAsync(client, options.BucketName!, storageKeyBuilder.Normalize(storageKey), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.Value.AmazonS3;
        EnsureBucketConfigured(options);

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        using var client = CreateClient(options);
        if (!await ObjectExistsAsync(client, options.BucketName!, normalizedKey, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        await client.DeleteObjectAsync(options.BucketName, normalizedKey, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<FileStorageFileMetadata> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.Value.AmazonS3;
        EnsureBucketConfigured(options);

        var normalizedKey = storageKeyBuilder.Normalize(storageKey);
        using var client = CreateClient(options);

        try
        {
            var response = await client.GetObjectMetadataAsync(options.BucketName, normalizedKey, cancellationToken).ConfigureAwait(false);
            return CreateMetadata(normalizedKey, response);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileStorageException($"No S3 object exists for storage key '{normalizedKey}'.", exception);
        }
    }

    /// <inheritdoc />
    public Task<Uri?> GetDownloadUriAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = optionsAccessor.Value.AmazonS3;
        EnsureBucketConfigured(options);

        using var client = CreateClient(options);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = options.BucketName,
            Key = storageKeyBuilder.Normalize(storageKey),
            Expires = DateTime.UtcNow.AddMinutes(Math.Max(1, options.PreSignedUrlExpirationMinutes)),
            Verb = HttpVerb.GET,
        };

        return Task.FromResult<Uri?>(new Uri(client.GetPreSignedURL(request), UriKind.Absolute));
    }

    private static IAmazonS3 CreateClient(AmazonS3FileStorageOptions options)
    {
        var config = new AmazonS3Config();

        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            config.ServiceURL = options.ServiceUrl;
            config.ForcePathStyle = true;
        }
        else if (!string.IsNullOrWhiteSpace(options.Region))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
        }

        if (!string.IsNullOrWhiteSpace(options.AccessKeyId) && !string.IsNullOrWhiteSpace(options.SecretAccessKey))
        {
            return new AmazonS3Client(new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey), config);
        }

        return new AmazonS3Client(config);
    }

    private static void EnsureBucketConfigured(AmazonS3FileStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new FileStorageException("AmazonS3.BucketName is required when Provider is AmazonS3.");
        }
    }

    private static async Task<bool> ObjectExistsAsync(IAmazonS3 client, string bucketName, string storageKey, CancellationToken cancellationToken)
    {
        try
        {
            await client.GetObjectMetadataAsync(bucketName, storageKey, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private static Uri? BuildObjectUri(AmazonS3FileStorageOptions options, string storageKey)
    {
        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            return new Uri($"{options.ServiceUrl.TrimEnd('/')}/{options.BucketName}/{Uri.EscapeDataString(storageKey).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)}");
        }

        if (string.IsNullOrWhiteSpace(options.Region))
        {
            return null;
        }

        return new Uri($"https://{options.BucketName}.s3.{options.Region}.amazonaws.com/{Uri.EscapeDataString(storageKey).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)}");
    }

    private static FileStorageFileMetadata CreateMetadata(string storageKey, GetObjectResponse response)
        => new()
        {
            ProviderName = FileStorageProviderNames.AmazonS3,
            StorageKey = storageKey,
            FileName = Path.GetFileName(storageKey),
            ContentType = response.Headers.ContentType,
            Length = response.Headers.ContentLength,
            LastModifiedUtc = response.LastModified?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
            Metadata = response.Metadata.Keys.ToDictionary(key => key, key => response.Metadata[key], StringComparer.OrdinalIgnoreCase),
        };

    private static FileStorageFileMetadata CreateMetadata(string storageKey, GetObjectMetadataResponse response)
        => new()
        {
            ProviderName = FileStorageProviderNames.AmazonS3,
            StorageKey = storageKey,
            FileName = Path.GetFileName(storageKey),
            ContentType = response.Headers.ContentType,
            Length = response.Headers.ContentLength,
            LastModifiedUtc = response.LastModified?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
            Metadata = response.Metadata.Keys.ToDictionary(key => key, key => response.Metadata[key], StringComparer.OrdinalIgnoreCase),
        };
}
