// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

namespace ChuA.FileStorage.Abstractions;

/// <summary>
/// Sanitizes caller-supplied file names before they become storage keys.
/// </summary>
public interface IFileNameSanitizer
{
    /// <summary>
    /// Sanitizes a file name.
    /// </summary>
    /// <param name="fileName">The caller-supplied file name.</param>
    /// <returns>A safe file name.</returns>
    string Sanitize(string fileName);
}
