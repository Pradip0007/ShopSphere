namespace ShopSphere.Domain.Storage;

public interface IFileStorage
{
    Task<string> UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken ct);

    string GetPublicUrl(string key);
}
