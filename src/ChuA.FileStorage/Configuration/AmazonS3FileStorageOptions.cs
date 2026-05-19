namespace ChuA.FileStorage.Configuration;

/// <summary>
/// Configuration shape for Amazon S3 provider implementations.
/// </summary>
public sealed class AmazonS3FileStorageOptions
{
    /// <summary>
    /// Gets or sets the AWS region system name.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Gets or sets the S3 bucket name.
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// Gets or sets an optional service URL for compatible S3 providers.
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Gets or sets the access key. Leave empty to use the standard AWS credential chain.
    /// </summary>
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// Gets or sets the secret key. Leave empty to use the standard AWS credential chain.
    /// </summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>
    /// Gets or sets the pre-signed URL lifetime in minutes.
    /// </summary>
    public int PreSignedUrlExpirationMinutes { get; set; } = 15;
}
