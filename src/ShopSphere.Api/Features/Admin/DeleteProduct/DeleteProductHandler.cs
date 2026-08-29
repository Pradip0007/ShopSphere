using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Admin.DeleteProduct;

public sealed class DeleteProductHandler(ShopSphereDbContext db)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        ProductId productId = new(request.Id);

        Product product = await db.Products
            .FirstOrDefaultAsync(
                p => p.Id == productId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Product {request.Id} not found.");

        product.Archive();

        await db.SaveChangesAsync(cancellationToken);
    }
}