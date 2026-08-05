using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Middleware;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Admin.CreateProduct;

public sealed class CreateProductHandler(ShopSphereDbContext db)
    : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        Category? category = await db.Categories
            .FirstOrDefaultAsync(
                c => c.Id.Value == request.CategoryId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
            $"Category {request.CategoryId} not found.");

            bool slugTaken = await db.Products
            .AnyAsync(
                p => p.Slug.Value == request.Slug,
                cancellationToken);

        if (slugTaken)
        {
            throw new ConflictException(
                $"Slug '{request.Slug}' is already in use.");
        }

        Product product = Product.Create(
            title: request.Title,
            description: request.Description,
            sku: Sku.From(Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()),
            categoryId: category.Id,
            price: new Money(request.Price, request.Currency),
            slug: Slug.From(request.Slug));

        db.Products.Add(product);

        db.StockLevels.Add(
            ShopSphere.Domain.Inventory.StockLevel.Create(
                product.Id,
                request.StockOnHand));

        await db.SaveChangesAsync(cancellationToken);

        return new CreateProductResponse(product.Id.Value);
    }
}