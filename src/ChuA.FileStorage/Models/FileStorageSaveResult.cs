// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

namespace ChuA.FileStorage.Models;

/// <summary>
/// Represents the result of a successful file save operation.
/// </summary>
public sealed class FileStorageSaveResult
{
    /// <summary>
    /// Gets or initializes the provider that stored the file.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Gets or initializes the provider-specific normalized storage key.
    /// </summary>
    public required string StorageKey { get; init; }

    /// <summary>
    /// Gets or initializes the stored file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or initializes the content type stored with the file.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets or initializes the number of bytes written.
    /// </summary>
    public long Length { get; init; }

    /// <summary>
    /// Gets or initializes the UTC timestamp when the file was stored.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets or initializes the URI callers may use to retrieve the file, when available.
    /// </summary>
    public Uri? Uri { get; init; }
}
