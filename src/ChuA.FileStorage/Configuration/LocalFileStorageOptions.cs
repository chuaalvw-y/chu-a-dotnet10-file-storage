// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

namespace ChuA.FileStorage.Configuration;

/// <summary>
/// Configuration for the local file system provider.
/// </summary>
public sealed class LocalFileStorageOptions
{
    /// <summary>
    /// Gets or sets the absolute or application-relative base path where files are stored.
    /// </summary>
    public string BasePath { get; set; } = "App_Data/FileStorage";

    /// <summary>
    /// Gets or sets whether the provider creates the base path when missing.
    /// </summary>
    public bool CreateDirectoryIfMissing { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional public base URI used to build file download links.
    /// </summary>
    public string? PublicBaseUri { get; set; }
}
