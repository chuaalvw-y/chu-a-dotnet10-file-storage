namespace ChuA.FileStorage.Constants;

/// <summary>
/// Default values used by ChuA.FileStorage.
/// </summary>
public static class FileStorageDefaults
{
    /// <summary>
    /// Default configuration section name used by the extension methods.
    /// </summary>
    public const string SectionName = "ChuA:FileStorage";

    /// <summary>
    /// Default maximum upload size: 100 MB.
    /// </summary>
    public const long MaxFileSizeBytes = 104_857_600;
}
