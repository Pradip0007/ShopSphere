using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Admin.PublishProduct;

public sealed class PublishProductHandler(ShopSphereDbContext db)
    : IRequestHandler<PublishProductCommand>
{
    public async Task Handle(
        PublishProductCommand request,
        CancellationToken cancellationToken)
    {
        ProductId productId = new(request.Id);
        Product product = await db.Products.FirstOrDefaultAsync(
                p => p.Id == productId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Product {request.Id} not found.");

        product.Publish();
        await db.SaveChangesAsync(cancellationToken);
    }
}
