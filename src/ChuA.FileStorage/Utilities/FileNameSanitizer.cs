// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Models;

namespace ChuA.FileStorage.Utilities;

/// <summary>
/// Default file name sanitizer.
/// </summary>
public sealed class FileNameSanitizer : IFileNameSanitizer
{
    private static readonly char[] InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();

    /// <inheritdoc />
    public string Sanitize(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new FileStorageException("A file name is required.");
        }

        var safeName = System.IO.Path.GetFileName(fileName.Trim());
        foreach (var invalidChar in InvalidFileNameChars)
        {
            safeName = safeName.Replace(invalidChar, '_');
        }

        safeName = safeName.Replace("..", "_", StringComparison.Ordinal);
        safeName = safeName.Trim('.', ' ');

        if (string.IsNullOrWhiteSpace(safeName))
        {
            throw new FileStorageException("The supplied file name is not valid after sanitization.");
        }

        return safeName;
    }
}
