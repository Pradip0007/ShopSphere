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
        Product product = await db.Products
            .FirstOrDefaultAsync(
                p => p.Id.Value == request.Id,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Product {request.Id} not found.");

        product.Archive();

        await db.SaveChangesAsync(cancellationToken);
    }
}