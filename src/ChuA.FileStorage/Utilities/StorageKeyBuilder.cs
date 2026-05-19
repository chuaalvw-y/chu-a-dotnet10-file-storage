using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Models;

namespace ChuA.FileStorage.Utilities;

/// <summary>
/// Default storage key normalizer.
/// </summary>
public sealed class StorageKeyBuilder : IStorageKeyBuilder
{
    /// <inheritdoc />
    public string Build(string? path, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new FileStorageException("A file name is required.");
        }

        var prefix = NormalizePath(path);
        return string.IsNullOrEmpty(prefix)
            ? Normalize(fileName)
            : Normalize($"{prefix}/{fileName}");
    }

    /// <inheritdoc />
    public string Normalize(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new FileStorageException("A storage key is required.");
        }

        var normalized = storageKey.Replace('\\', '/').Trim('/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0 || segments.Any(IsUnsafeSegment))
        {
            throw new FileStorageException("The storage key must be relative and cannot contain traversal segments.");
        }

        return string.Join('/', segments);
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path.Replace('\\', '/').Trim('/');
    }

    private static bool IsUnsafeSegment(string segment)
    {
        return segment is "." or ".."
            || segment.Contains(':', StringComparison.Ordinal)
            || System.IO.Path.IsPathFullyQualified(segment);
    }
}
