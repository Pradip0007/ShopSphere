using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using ShopSphere.Domain.Storage;

namespace ShopSphere.Infrastructure.Storage;

public sealed class AzureBlobFileStorage(
    BlobContainerClient container,
    IOptions<StorageOptions> options) : IFileStorage
{
    private readonly StorageOptions _options = options.Value;

    public async Task<string> UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken ct)
    {
        var blob = container.GetBlobClient(key);

        await blob.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            },
            ct);

        return key;
    }

    public string GetPublicUrl(string key)
    {
        return $"{_options.CdnBaseUrl.TrimEnd('/')}/{key}";
    }
}
