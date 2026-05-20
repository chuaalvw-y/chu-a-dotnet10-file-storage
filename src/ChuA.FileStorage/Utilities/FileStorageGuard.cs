// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using ChuA.FileStorage.Configuration;
using ChuA.FileStorage.Models;

namespace ChuA.FileStorage.Utilities;

/// <summary>
/// Guard helpers for file storage operations.
/// </summary>
public static class FileStorageGuard
{
    /// <summary>
    /// Validates a save request against configured limits.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <param name="options">The storage options.</param>
    public static void ValidateSaveRequest(FileStorageSaveRequest request, FileStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        if (request.Content is null)
        {
            throw new FileStorageException("File content is required.");
        }

        if (!request.Content.CanRead)
        {
            throw new FileStorageException("File content must be readable.");
        }

        if (request.Length is > 0 && request.Length > options.MaxFileSizeBytes)
        {
            throw new FileStorageException($"The file exceeds the configured maximum size of {options.MaxFileSizeBytes} bytes.");
        }

        if (request.Content.CanSeek && request.Content.Length > options.MaxFileSizeBytes)
        {
            throw new FileStorageException($"The file exceeds the configured maximum size of {options.MaxFileSizeBytes} bytes.");
        }

        if (options.AllowedContentTypes.Count > 0
            && (string.IsNullOrWhiteSpace(request.ContentType)
                || !options.AllowedContentTypes.Contains(request.ContentType)))
        {
            throw new FileStorageException("The file content type is not allowed.");
        }
    }
}
