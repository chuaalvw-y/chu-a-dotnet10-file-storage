namespace ChuA.FileStorage.Abstractions;

/// <summary>
/// Builds and validates provider-neutral storage keys.
/// </summary>
public interface IStorageKeyBuilder
{
    /// <summary>
    /// Builds a normalized storage key from a relative path and file name.
    /// </summary>
    /// <param name="path">An optional relative path.</param>
    /// <param name="fileName">The sanitized file name.</param>
    /// <returns>A normalized storage key.</returns>
    string Build(string? path, string fileName);

    /// <summary>
    /// Normalizes a storage key supplied by a caller.
    /// </summary>
    /// <param name="storageKey">The storage key.</param>
    /// <returns>A normalized storage key.</returns>
    string Normalize(string storageKey);
}
