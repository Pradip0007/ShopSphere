namespace ShopSphere.Infrastructure.Storage;

public sealed class StorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "product-images";

    public string CdnBaseUrl { get; set; } = string.Empty;
}