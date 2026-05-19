using ChuA.FileStorage.Constants;
using Microsoft.Extensions.Options;

namespace ChuA.FileStorage.Configuration;

/// <summary>
/// Validates <see cref="FileStorageOptions"/> for safe startup behavior.
/// </summary>
public sealed class FileStorageOptionsValidator : IValidateOptions<FileStorageOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, FileStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            failures.Add("Provider is required.");
        }

        if (options.MaxFileSizeBytes <= 0)
        {
            failures.Add("MaxFileSizeBytes must be greater than zero.");
        }

        if (options.Provider.Equals(FileStorageProviderNames.Local, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(options.Local.BasePath))
        {
            failures.Add("Local.BasePath is required when Provider is Local.");
        }

        if (options.Local.PublicBaseUri is not null && !Uri.TryCreate(options.Local.PublicBaseUri, UriKind.Absolute, out _))
        {
            failures.Add("Local.PublicBaseUri must be an absolute URI when provided.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
