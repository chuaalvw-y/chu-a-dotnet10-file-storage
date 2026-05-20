// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Constants;
using ChuA.FileStorage.Extensions;
using ChuA.FileStorage.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChuA.FileStorage.Tests.Extensions;

public sealed class FileStorageServiceCollectionExtensionsTests
{
    [Fact]
    public void AddChuAFileStorage_RegistersCoreServices()
    {
        using var provider = CreateProvider(FileStorageProviderNames.Local);

        Assert.NotNull(provider.GetRequiredService<IFileStorageService>());
        Assert.NotNull(provider.GetRequiredService<IFileStorageProviderFactory>());
        Assert.NotNull(provider.GetRequiredService<IFileNameSanitizer>());
        Assert.NotNull(provider.GetRequiredService<IStorageKeyBuilder>());
    }

    [Theory]
    [InlineData(FileStorageProviderNames.Local)]
    [InlineData(FileStorageProviderNames.AzureBlob)]
    [InlineData(FileStorageProviderNames.AmazonS3)]
    public void ProviderFactory_ReturnsConfiguredProvider(string providerName)
    {
        using var provider = CreateProvider(providerName);

        var resolvedProvider = provider.GetRequiredService<IFileStorageProviderFactory>().GetProvider();

        Assert.Equal(providerName, resolvedProvider.Name);
    }

    [Fact]
    public async Task ConfiguredAzureProviderWithoutContainer_FailsSafely()
    {
        using var provider = CreateProvider(FileStorageProviderNames.AzureBlob);
        var storage = provider.GetRequiredService<IFileStorageService>();

        var exception = await Assert.ThrowsAsync<FileStorageException>(() =>
            storage.ExistsAsync("documents/report.pdf"));

        Assert.Contains("AzureBlob.ContainerName", exception.Message);
    }

    [Fact]
    public async Task ConfiguredAmazonS3ProviderWithoutBucket_FailsSafely()
    {
        using var provider = CreateProvider(FileStorageProviderNames.AmazonS3);
        var storage = provider.GetRequiredService<IFileStorageService>();

        var exception = await Assert.ThrowsAsync<FileStorageException>(() =>
            storage.ExistsAsync("documents/report.pdf"));

        Assert.Contains("AmazonS3.BucketName", exception.Message);
    }

    [Fact]
    public void AddChuAFileStorage_RejectsBlankSectionName()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentException>(() =>
            new ServiceCollection().AddChuAFileStorage(configuration, " "));
    }

    private static ServiceProvider CreateProvider(string providerName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ChuA:FileStorage:Provider"] = providerName,
                ["ChuA:FileStorage:Local:BasePath"] = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")),
            })
            .Build();

        return new ServiceCollection()
            .AddChuAFileStorage(configuration)
            .BuildServiceProvider();
    }
}
