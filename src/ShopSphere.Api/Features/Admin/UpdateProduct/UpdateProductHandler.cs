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
        CancellationToken cancellationToken)
    {
        Product product = await db.Products
            .FirstOrDefaultAsync(
                p => p.Id.Value == request.Id,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Product {request.Id} not found.");

        if (request.Title is not null || request.Slug is not null)
        {
            product.Rename(
                request.Title ?? product.Title,
                request.Slug is null
                    ? product.Slug
                    : Slug.From(request.Slug));
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

        if (request.StockOnHand.HasValue)
        {
            var stock = await db.StockLevels
                .FirstOrDefaultAsync(
                    s => s.ProductId == product.Id,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"Stock for product {request.Id} not found.");

            int delta = request.StockOnHand.Value - stock.Available;

            Result result = stock.Adjust(delta);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error.Message);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}