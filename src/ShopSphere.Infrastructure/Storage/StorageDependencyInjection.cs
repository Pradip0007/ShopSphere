using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShopSphere.Domain.Storage;

namespace ShopSphere.Infrastructure.Storage;

public static class StorageDependencyInjection
{
    public static IServiceCollection AddFileStorage(
        this IServiceCollection services)
    {
        services
            .AddOptions<StorageOptions>()
            .BindConfiguration("Storage")
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.ConnectionString),
                "Blob Storage connection string is required.")
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.ContainerName),
                "Storage:ContainerName is required.")
            .ValidateOnStart();

        services.AddSingleton<BlobContainerClient>(sp =>
        {
            var options =
                sp.GetRequiredService<IOptions<StorageOptions>>().Value;

            return new BlobContainerClient(
                options.ConnectionString,
                options.ContainerName);
        });

        services.AddScoped<IFileStorage, AzureBlobFileStorage>();

        return services;
    }
}