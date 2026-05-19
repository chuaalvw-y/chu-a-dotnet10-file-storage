namespace ChuA.FileStorage.Configuration;

/// <summary>
/// Configuration shape for Azure Blob Storage provider implementations.
/// </summary>
public sealed class AzureBlobFileStorageOptions
{
    /// <summary>
    /// Gets or sets the Azure Storage connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the blob container name.
    /// </summary>
    public string? ContainerName { get; set; }

    /// <summary>
    /// Gets or sets an optional service endpoint for managed identity or custom clients.
    /// </summary>
    public string? ServiceUri { get; set; }

    /// <summary>
    /// Gets or sets whether the blob container should be created when missing.
    /// </summary>
    public bool CreateContainerIfMissing { get; set; } = true;
}
