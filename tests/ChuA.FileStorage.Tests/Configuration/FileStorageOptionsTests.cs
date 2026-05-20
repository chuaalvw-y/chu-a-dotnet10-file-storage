// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

using ChuA.FileStorage.Configuration;
using ChuA.FileStorage.Constants;
using ChuA.FileStorage.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChuA.FileStorage.Tests.Configuration;

public sealed class FileStorageOptionsTests
{
    [Fact]
    public void AddChuAFileStorage_BindsOptionsFromDefaultSection()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["ChuA:FileStorage:Provider"] = FileStorageProviderNames.Local,
            ["ChuA:FileStorage:MaxFileSizeBytes"] = "1024",
            ["ChuA:FileStorage:AllowOverwrite"] = "true",
            ["ChuA:FileStorage:AllowedContentTypes:0"] = "text/plain",
            ["ChuA:FileStorage:Local:BasePath"] = "storage",
        });

        var options = provider.GetRequiredService<IOptions<FileStorageOptions>>().Value;

        Assert.Equal(FileStorageProviderNames.Local, options.Provider);
        Assert.Equal(1024, options.MaxFileSizeBytes);
        Assert.True(options.AllowOverwrite);
        Assert.Contains("text/plain", options.AllowedContentTypes);
        Assert.Equal("storage", options.Local.BasePath);
    }

    [Fact]
    public void AddChuAFileStorage_BindsOptionsFromCustomSection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = FileStorageProviderNames.Local,
                ["Storage:Local:BasePath"] = "custom-storage",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddChuAFileStorage(configuration, "Storage")
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<FileStorageOptions>>().Value;

        Assert.Equal("custom-storage", options.Local.BasePath);
    }

    [Fact]
    public void OptionsValidation_FailsForInvalidLocalConfiguration()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["ChuA:FileStorage:Provider"] = FileStorageProviderNames.Local,
            ["ChuA:FileStorage:Local:BasePath"] = "",
        });

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<FileStorageOptions>>().Value);

        Assert.Contains("Local.BasePath is required", exception.Message);
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new ServiceCollection()
            .AddChuAFileStorage(configuration)
            .BuildServiceProvider();
    }
}
