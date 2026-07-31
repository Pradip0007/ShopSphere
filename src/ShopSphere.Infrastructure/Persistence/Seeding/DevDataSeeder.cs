using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory;

namespace ShopSphere.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotent development seed — safe to run on every Api startup.
/// Skips work if the categories table already has rows.
/// </summary>
public static class DevDataSeeder
{
    public static async Task SeedAsync(
        ShopSphereDbContext db,
        ILogger logger,
        CancellationToken ct = default)
    {
        if (await db.Categories.AnyAsync(ct))
        {
            logger.LogInformation("DevDataSeeder: categories already present, skipping.");
            return;
        }

        logger.LogInformation("DevDataSeeder: seeding baseline data…");

        // --- Categories ------------------------------------------------------
        var electronics = Category.Create("Electronics");
        var books = Category.Create("Books");
        var clothing = Category.Create("Clothing");

        db.Categories.AddRange(electronics, books, clothing);

        // --- Products --------------------------------------------------------
        var gbp = "GBP";
        var products = new[]
        {
            Product.Create("Wireless Headphones", "Over-ear, 40h battery.",
                Sku.From("ELEC-HDPH-001"), electronics.Id, new Money(129.99m, gbp)),
            Product.Create("Bluetooth Speaker", "Portable, waterproof.",
                Sku.From("ELEC-SPKR-001"), electronics.Id, new Money(59.50m, gbp)),
            Product.Create("USB-C Cable 2m", "Braided, 100W PD.",
                Sku.From("ELEC-CBL-002"), electronics.Id, new Money(9.99m, gbp)),
            Product.Create("Domain-Driven Design", "Blue book. Evans, 2003.",
                Sku.From("BOOK-DDD-001"), books.Id, new Money(45.00m, gbp)),
            Product.Create("Clean Architecture", "Martin, 2017.",
                Sku.From("BOOK-CLNARCH-001"), books.Id, new Money(38.00m, gbp)),
            Product.Create("Merino Wool T-Shirt", "Grey, size M.",
                Sku.From("CLTH-TSHRT-M-001"), clothing.Id, new Money(48.00m, gbp)),
            Product.Create("Waterproof Jacket", "3-layer, size L.",
                Sku.From("CLTH-JKT-L-001"), clothing.Id, new Money(180.00m, gbp)),
        };

        // Publish half of them so lists have something to show.
        foreach (var p in products.Take(4))
            p.Publish();

        db.Products.AddRange(products);

        // --- Stock levels ----------------------------------------------------
        foreach (var p in products)
        {
            var stock = StockLevel.Create(p.Id, initialAvailable: Random.Shared.Next(5, 40));
            db.StockLevels.Add(stock);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "DevDataSeeder: seeded {CategoryCount} categories, {ProductCount} products, {StockCount} stock rows.",
            3, products.Length, products.Length);
    }
}
