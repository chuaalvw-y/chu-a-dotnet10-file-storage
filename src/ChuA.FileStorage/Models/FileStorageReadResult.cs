// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

namespace ChuA.FileStorage.Models;

/// <summary>
/// Represents an opened file stream and its metadata.
/// </summary>
public sealed class FileStorageReadResult : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets or initializes the content stream.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// Gets or initializes metadata for the opened file.
    /// </summary>
    public required FileStorageFileMetadata Metadata { get; init; }

    /// <inheritdoc />
    public void Dispose() => Content.Dispose();

    /// <inheritdoc />
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
