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
