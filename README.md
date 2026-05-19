# ChuA.FileStorage

ChuA.FileStorage is a reusable .NET 10 file storage library for ASP.NET Core Web API, MVC, Razor Pages, Blazor Server, worker services, and internal enterprise applications.

It provides one application-facing abstraction over local file system storage, Azure Blob Storage, Amazon S3, and S3-compatible services.

## Architecture

The library is organized around clean, testable boundaries:

- `IFileStorageService` is the primary application API.
- `IFileStorageProvider` is the provider contract for local, cloud, or custom storage backends.
- `IFileStorageProviderFactory` selects the configured provider.
- Strongly typed options are bound through the Options pattern.
- Default utilities sanitize file names, normalize storage keys, and reject unsafe traversal.
- Local, Azure Blob Storage, and Amazon S3 providers are included out of the box.

## Installation

Reference the project or package from your .NET 10 application, then add configuration and one service registration call:

```csharp
builder.Services.AddChuAFileStorage(builder.Configuration);
```

Or use a custom configuration section:

```csharp
builder.Services.AddChuAFileStorage(builder.Configuration, "Storage");
```

## appsettings.json

```json
{
  "ChuA": {
    "FileStorage": {
      "Provider": "Local",
      "MaxFileSizeBytes": 104857600,
      "AllowOverwrite": false,
      "AllowedContentTypes": [
        "image/png",
        "image/jpeg",
        "application/pdf"
      ],
      "Local": {
        "BasePath": "App_Data/FileStorage",
        "CreateDirectoryIfMissing": true,
        "PublicBaseUri": "https://files.example.com/"
      },
      "AzureBlob": {
        "ConnectionString": "",
        "ContainerName": "",
        "ServiceUri": "",
        "CreateContainerIfMissing": true
      },
      "AmazonS3": {
        "Region": "",
        "BucketName": "",
        "ServiceUrl": "",
        "AccessKeyId": "",
        "SecretAccessKey": "",
        "PreSignedUrlExpirationMinutes": 15
      }
    }
  }
}
```

## Program.cs

```csharp
using ChuA.FileStorage.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddChuAFileStorage(builder.Configuration);

var app = builder.Build();

app.MapControllers();
app.Run();
```

## Usage

```csharp
using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Models;

public sealed class DocumentService
{
    private readonly IFileStorageService fileStorage;

    public DocumentService(IFileStorageService fileStorage)
    {
        this.fileStorage = fileStorage;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        var result = await fileStorage.SaveAsync(new FileStorageSaveRequest
        {
            Content = content,
            FileName = fileName,
            Path = "documents",
            ContentType = "application/pdf",
            Length = content.CanSeek ? content.Length : null,
        }, cancellationToken);

        return result.StorageKey;
    }
}
```

## Extension Points

Built-in provider names are `Local`, `AzureBlob`, and `AmazonS3`.

Add another provider by implementing `IFileStorageProvider` and registering it with dependency injection:

```csharp
services.AddSingleton<IFileStorageProvider, MyEnterpriseStorageProvider>();
```

Set `ChuA:FileStorage:Provider` to the provider name returned by your implementation.

## Production Defaults

- File names are sanitized.
- Storage keys must be relative.
- Path traversal is rejected.
- File size is limited by configuration.
- Content type allow-listing is available.
- Overwrite is disabled by default.
- Provider failures use `FileStorageException` with safe messages.

## Build and Test

```powershell
dotnet build ChuA.FileStorage.slnx
dotnet test ChuA.FileStorage.slnx
```
