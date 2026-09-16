using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.IntegrationTests.Catalog;

internal static class CatalogSeed
{
    public static async Task<Category> AddCategory(
        ShopSphereDbContext db,
        string name = "Widgets")
    {
        var category = Category.Create(name);
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public static async Task<Product> AddPublishedProduct(
        ShopSphereDbContext db,
        string sku = "SKU-1",
        string title = "Widget",
        decimal price = 9.99m,
        CategoryId? categoryId = null,
        string description = "A useful widget")
    {
        CategoryId resolvedCategoryId = categoryId ?? (await AddCategory(db)).Id;
        var product = Product.Create(
            title,
            description,
            Sku.From(sku),
            resolvedCategoryId,
            new Money(price, "USD"));
        product.Publish();
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public static async Task<Product> AddDraftProduct(
        ShopSphereDbContext db,
        CategoryId categoryId,
        string sku = "DRAFT-1",
        string title = "Draft Widget",
        decimal price = 9.99m)
    {
        var product = Product.Create(
            title,
            "Draft description",
            Sku.From(sku),
            categoryId,
            new Money(price, "USD"));
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public static async Task SeedManyPublished(
        ShopSphereDbContext db,
        int count,
        CategoryId categoryId)
    {
        for (var i = 0; i < count; i++)
        {
            var product = Product.Create(
                $"Product {i:D2}",
                $"Description {i}",
                Sku.From($"SKU-{i:D4}"),
                categoryId,
                new Money(1m + i, "USD"));
            product.Publish();
            db.Products.Add(product);
        }

        await db.SaveChangesAsync();
    }
}
