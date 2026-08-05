using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Admin.UpdateProduct;

public sealed class UpdateProductHandler(ShopSphereDbContext db)
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(
        UpdateProductCommand request,
        CancellationToken ct)
    {
        ProductId id  = new (request.Id);
        Product product = await db.Products.FirstOrDefaultAsync(
                p => p.Id == id,ct)
            ?? throw new KeyNotFoundException(
                $"Product {request.Id} not found.");

        if (request.Title is not null)
        {
            product.Rename(request.Title);
        }

        if (request.Description is not null)
        {
            product.UpdateDescription(request.Description);
        }

        if (request.Price.HasValue && request.Currency is not null)
        {
            product.ChangePrice(
                new Money(request.Price.Value, request.Currency));
        }

        await db.SaveChangesAsync(ct);
    }
}