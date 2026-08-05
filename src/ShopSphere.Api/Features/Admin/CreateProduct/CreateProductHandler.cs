using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Middleware;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Admin.CreateProduct;

public sealed class CreateProductHandler(ShopSphereDbContext db)
    : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(
        CreateProductCommand request,
        CancellationToken ct)
    {
        CategoryId categoryId = new (request.CategoryId);
        bool categoryExists = await db.Categories
                                .AnyAsync(c => c.Id == categoryId, ct);
        if (!categoryExists)
            throw new KeyNotFoundException($"Category {request.CategoryId} not found.");

        bool skuTaken = await db.Products
                        .AnyAsync(p => p.Sku == Sku.From(request.Sku),ct);
        if(skuTaken)
            throw new ConflictException($"SKU '{request.Sku}' is already in use.");

        Product product = Product.Create(
            title: request.Title,
            description: request.Description,
            sku: Sku.From(request.Sku),
            categoryId: categoryId,
            price: new Money(request.Price, request.Currency));
        
        StockLevel stock = StockLevel.Create(product.Id, request.InitialStock);

        db.Products.Add(product);

        db.StockLevels.Add(stock);

        await db.SaveChangesAsync(ct);

        return new CreateProductResponse(product.Id.Value);
    }
}