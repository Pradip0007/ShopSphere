namespace ShopSphere.Api.Features.Catalog.UploadProductImage;

public sealed record UploadProductImageResponse(
    Guid ProductId,
    string FileName,
    string ContentType,
    long Size,
    string BlobKey,
    string Url
);