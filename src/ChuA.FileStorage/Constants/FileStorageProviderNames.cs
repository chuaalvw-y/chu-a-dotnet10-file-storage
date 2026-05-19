namespace ChuA.FileStorage.Constants;

/// <summary>
/// Well-known provider names understood by ChuA.FileStorage.
/// </summary>
public static class FileStorageProviderNames
{
    /// <summary>
    /// Stores files on the local file system.
    /// </summary>
    public const string Local = "Local";

    /// <summary>
    /// Identifies an Azure Blob Storage provider implementation.
    /// </summary>
    public const string AzureBlob = "AzureBlob";

    /// <summary>
    /// Identifies an Amazon S3 provider implementation.
    /// </summary>
    public const string AmazonS3 = "AmazonS3";
}
