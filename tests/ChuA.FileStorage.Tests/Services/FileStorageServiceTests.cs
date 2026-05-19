using System.Text;
using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Constants;
using ChuA.FileStorage.Extensions;
using ChuA.FileStorage.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChuA.FileStorage.Tests.Services;

public sealed class FileStorageServiceTests : IDisposable
{
    private readonly string tempPath = Path.Combine(Path.GetTempPath(), "ChuA.FileStorage.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SaveReadMetadataAndDelete_WorkForLocalProvider()
    {
        using var provider = CreateProvider();
        var storage = provider.GetRequiredService<IFileStorageService>();
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("hello storage"));

        var saved = await storage.SaveAsync(new FileStorageSaveRequest
        {
            Content = content,
            FileName = "report.txt",
            Path = "documents/2026",
            ContentType = "text/plain",
            Length = content.Length,
        });

        Assert.Equal("documents/2026/report.txt", saved.StorageKey);
        Assert.Equal(content.Length, saved.Length);
        Assert.True(await storage.ExistsAsync(saved.StorageKey));

        await using (var read = await storage.OpenReadAsync(saved.StorageKey))
        {
            using var reader = new StreamReader(read.Content, Encoding.UTF8);
            Assert.Equal("hello storage", await reader.ReadToEndAsync());
        }

        var metadata = await storage.GetMetadataAsync(saved.StorageKey);
        Assert.Equal(saved.StorageKey, metadata.StorageKey);
        Assert.Equal(content.Length, metadata.Length);

        Assert.True(await storage.DeleteAsync(saved.StorageKey));
        Assert.False(await storage.ExistsAsync(saved.StorageKey));
        Assert.False(await storage.DeleteAsync(saved.StorageKey));
    }

    [Fact]
    public async Task SaveAsync_RejectsPathTraversal()
    {
        using var provider = CreateProvider();
        var storage = provider.GetRequiredService<IFileStorageService>();
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Assert.ThrowsAsync<FileStorageException>(() =>
            storage.SaveAsync(new FileStorageSaveRequest
            {
                Content = content,
                FileName = "secret.txt",
                Path = "../outside",
                ContentType = "text/plain",
            }));

        Assert.Contains("traversal", exception.Message);
    }

    [Fact]
    public async Task SaveAsync_RejectsFilesOverConfiguredLimit()
    {
        using var provider = CreateProvider(maxFileSizeBytes: 2);
        var storage = provider.GetRequiredService<IFileStorageService>();
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Assert.ThrowsAsync<FileStorageException>(() =>
            storage.SaveAsync(new FileStorageSaveRequest
            {
                Content = content,
                FileName = "large.bin",
                ContentType = "application/octet-stream",
                Length = content.Length,
            }));

        Assert.Contains("maximum size", exception.Message);
    }

    [Fact]
    public async Task SaveAsync_RejectsDisallowedContentType()
    {
        using var provider = CreateProvider(allowedContentType: "text/plain");
        var storage = provider.GetRequiredService<IFileStorageService>();
        await using var content = new MemoryStream([1]);

        var exception = await Assert.ThrowsAsync<FileStorageException>(() =>
            storage.SaveAsync(new FileStorageSaveRequest
            {
                Content = content,
                FileName = "image.png",
                ContentType = "image/png",
                Length = content.Length,
            }));

        Assert.Contains("content type", exception.Message);
    }

    [Fact]
    public async Task SaveAsync_RejectsOverwriteWhenDisabled()
    {
        using var provider = CreateProvider();
        var storage = provider.GetRequiredService<IFileStorageService>();

        await storage.SaveAsync(CreateTextRequest("same.txt", "first"));

        var exception = await Assert.ThrowsAsync<FileStorageException>(() =>
            storage.SaveAsync(CreateTextRequest("same.txt", "second")));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task GetDownloadUriAsync_UsesConfiguredPublicBaseUri()
    {
        using var provider = CreateProvider(publicBaseUri: "https://files.example.test/downloads/");
        var storage = provider.GetRequiredService<IFileStorageService>();

        var uri = await storage.GetDownloadUriAsync("documents/report.txt");

        Assert.Equal("https://files.example.test/downloads/documents/report.txt", uri?.ToString());
    }

    public void Dispose()
    {
        if (Directory.Exists(tempPath))
        {
            Directory.Delete(tempPath, recursive: true);
        }
    }

    private ServiceProvider CreateProvider(
        long maxFileSizeBytes = FileStorageDefaults.MaxFileSizeBytes,
        string? allowedContentType = null,
        string? publicBaseUri = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["ChuA:FileStorage:Provider"] = FileStorageProviderNames.Local,
            ["ChuA:FileStorage:MaxFileSizeBytes"] = maxFileSizeBytes.ToString(),
            ["ChuA:FileStorage:Local:BasePath"] = tempPath,
            ["ChuA:FileStorage:Local:PublicBaseUri"] = publicBaseUri,
        };

        if (!string.IsNullOrWhiteSpace(allowedContentType))
        {
            values["ChuA:FileStorage:AllowedContentTypes:0"] = allowedContentType;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new ServiceCollection()
            .AddChuAFileStorage(configuration)
            .BuildServiceProvider();
    }

    private static FileStorageSaveRequest CreateTextRequest(string fileName, string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return new FileStorageSaveRequest
        {
            Content = stream,
            FileName = fileName,
            ContentType = "text/plain",
            Length = stream.Length,
        };
    }
}
