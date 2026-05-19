using ChuA.FileStorage.Abstractions;
using ChuA.FileStorage.Configuration;
using ChuA.FileStorage.Constants;
using ChuA.FileStorage.Providers;
using ChuA.FileStorage.Services;
using ChuA.FileStorage.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ChuA.FileStorage.Extensions;

/// <summary>
/// Extension methods for registering ChuA.FileStorage.
/// </summary>
public static class FileStorageServiceCollectionExtensions
{
    /// <summary>
    /// Registers ChuA.FileStorage using the default configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddChuAFileStorage(this IServiceCollection services, IConfiguration configuration)
        => services.AddChuAFileStorage(configuration, FileStorageDefaults.SectionName);

    /// <summary>
    /// Registers ChuA.FileStorage using a custom configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddChuAFileStorage(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentException("A configuration section name is required.", nameof(sectionName));
        }

        services
            .AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateOnStart();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<FileStorageOptions>, FileStorageOptionsValidator>());
        services.TryAddSingleton<IFileNameSanitizer, FileNameSanitizer>();
        services.TryAddSingleton<IStorageKeyBuilder, StorageKeyBuilder>();
        services.TryAddSingleton<IFileStorageProviderFactory, FileStorageProviderFactory>();
        services.TryAddSingleton<IFileStorageService, FileStorageService>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFileStorageProvider, LocalFileStorageProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFileStorageProvider, AzureBlobFileStorageProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFileStorageProvider, AmazonS3FileStorageProvider>());

        return services;
    }
}
