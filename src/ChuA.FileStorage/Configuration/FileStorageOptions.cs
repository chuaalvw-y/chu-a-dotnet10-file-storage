// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using ChuA.FileStorage.Constants;

namespace ChuA.FileStorage.Configuration;

/// <summary>
/// Root configuration options for ChuA.FileStorage.
/// </summary>
public sealed class FileStorageOptions
{
    /// <summary>
    /// Gets or sets the active storage provider name.
    /// </summary>
    public string Provider { get; set; } = FileStorageProviderNames.Local;

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = FileStorageDefaults.MaxFileSizeBytes;

    /// <summary>
    /// Gets or sets whether save operations may overwrite existing files by default.
    /// </summary>
    public bool AllowOverwrite { get; set; }

    /// <summary>
    /// Gets or sets allowed content types. Empty means all content types are accepted.
    /// </summary>
    public HashSet<string> AllowedContentTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets local file system provider options.
    /// </summary>
    public LocalFileStorageOptions Local { get; set; } = new();

    /// <summary>
    /// Gets or sets Azure Blob Storage provider options.
    /// </summary>
    public AzureBlobFileStorageOptions AzureBlob { get; set; } = new();

    /// <summary>
    /// Gets or sets Amazon S3 provider options.
    /// </summary>
    public AmazonS3FileStorageOptions AmazonS3 { get; set; } = new();
}
