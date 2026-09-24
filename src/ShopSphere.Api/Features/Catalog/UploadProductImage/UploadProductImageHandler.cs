using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Storage;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Catalog.UploadProductImage;

public sealed class UploadProductImageHandler(
    ShopSphereDbContext db,
    IFileStorage storage)
    : IRequestHandler<UploadProductImageCommand, UploadProductImageResponse>
{
    public async Task<UploadProductImageResponse> Handle(
        UploadProductImageCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify product exists
        var productId = new ProductId(request.ProductId);

        var productExists = await db.Products
            .AnyAsync(
                p => p.Id == productId,
                cancellationToken);

        if (!productExists)
        {
            throw new KeyNotFoundException(
                $"Product '{request.ProductId}' was not found.");
        }

        // 2. Determine position
        var position = await db.ProductImages
        .Where(x => x.ProductId == productId)
        .Select(x => (int?)x.Position)
        .MaxAsync(cancellationToken) ?? -1;

        position++;

        // 3. Generate blob key
        var extension = Path.GetExtension(request.File.FileName);

        var blobKey =
            $"products/{request.ProductId}/{Guid.NewGuid():N}{extension}";

        // 4. Upload blob
        await using var stream = request.File.OpenReadStream();

        await storage.UploadAsync(
            blobKey,
            stream,
            request.File.ContentType,
            cancellationToken);

        // 5. Create domain entity using factory
        var image = ProductImage.For(
        productId,
        blobKey,
        request.File.ContentType,
        0,
        0,
        position);

        // 6. Add entity
        db.ProductImages.Add(image);

        // 7. Persist ProductImage row
        await db.SaveChangesAsync(cancellationToken);

        // 8. Return response
        return new UploadProductImageResponse(
            request.ProductId,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            blobKey,
            storage.GetPublicUrl(blobKey));
    }
}