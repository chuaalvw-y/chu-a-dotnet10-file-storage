// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Configuration;
using ChuA.FileStorage.Models;
using Microsoft.Extensions.Options;

namespace ChuA.FileStorage.Services;

/// <summary>
/// Default provider factory backed by dependency injection.
/// </summary>
public sealed class FileStorageProviderFactory : IFileStorageProviderFactory
{
    private readonly IReadOnlyDictionary<string, IFileStorageProvider> providers;
    private readonly IOptions<FileStorageOptions> optionsAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageProviderFactory"/> class.
    /// </summary>
    /// <param name="providers">Registered storage providers.</param>
    /// <param name="optionsAccessor">The configured options.</param>
    public FileStorageProviderFactory(IEnumerable<IFileStorageProvider> providers, IOptions<FileStorageOptions> optionsAccessor)
    {
        this.providers = providers
            .GroupBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
        this.optionsAccessor = optionsAccessor;
    }

    /// <inheritdoc />
    public IFileStorageProvider GetProvider()
    {
        var providerName = optionsAccessor.Value.Provider;
        if (providers.TryGetValue(providerName, out var provider))
        {
            return provider;
        }

        throw new FileStorageException($"No file storage provider is registered for '{providerName}'.");
    }
}
