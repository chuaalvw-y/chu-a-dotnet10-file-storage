namespace ChuA.FileStorage.Models;

/// <summary>
/// Describes a file stored by a provider.
/// </summary>
public sealed class FileStorageFileMetadata
{
    /// <summary>
    /// Gets or initializes the provider that owns the file.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Gets or initializes the normalized storage key.
    /// </summary>
    public required string StorageKey { get; init; }

    /// <summary>
    /// Gets or initializes the file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or initializes the content type, when known.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets or initializes the file length in bytes.
    /// </summary>
    public long Length { get; init; }

    /// <summary>
    /// Gets or initializes the UTC timestamp when the file was last modified.
    /// </summary>
    public DateTimeOffset LastModifiedUtc { get; init; }

    /// <summary>
    /// Gets or initializes provider-specific metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
