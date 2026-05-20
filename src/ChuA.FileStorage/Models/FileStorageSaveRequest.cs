// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

namespace ChuA.FileStorage.Models;

/// <summary>
/// Describes a file save operation.
/// </summary>
public sealed class FileStorageSaveRequest
{
    /// <summary>
    /// Gets or initializes the file content stream.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// Gets or initializes the original file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or initializes an optional relative folder or key prefix.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Gets or initializes the content type supplied by the caller.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets or initializes the expected content length in bytes.
    /// </summary>
    public long? Length { get; init; }

    /// <summary>
    /// Gets or initializes whether an existing file can be overwritten.
    /// </summary>
    public bool? Overwrite { get; init; }

    /// <summary>
    /// Gets or initializes caller-defined metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
