// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

namespace ChuA.FileStorage.Abstractions;

/// <summary>
/// Resolves the active storage provider from configuration.
/// </summary>
public interface IFileStorageProviderFactory
{
    /// <summary>
    /// Gets the currently configured provider.
    /// </summary>
    /// <returns>The active provider.</returns>
    IFileStorageProvider GetProvider();
}
