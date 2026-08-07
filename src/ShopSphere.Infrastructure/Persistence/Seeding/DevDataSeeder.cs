using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory;
using ShopSphere.Domain.Users;

namespace ShopSphere.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotent development seed.
/// Safe to run every application startup.
/// </summary>
public static class DevDataSeeder
{
    public static async Task SeedAsync(
        ShopSphereDbContext db,
        ILogger logger,
        CancellationToken ct = default)
    {
        // ---------------------------------------------------------------------
        // Seed Roles & Permissions
        // ---------------------------------------------------------------------
        if (!await db.Permissions.AnyAsync(ct))
        {
            logger.LogInformation("Seeding permissions and roles...");

            Permission[] permissions =
            [
                .. ShopSphere.Domain.Users.Permissions.All
                    .Select(Permission.Create)
            ];

            db.Permissions.AddRange(permissions);

            Role admin = Role.Create("admin");
            admin.Grant(permissions.Single(p => p.Name == ShopSphere.Domain.Users.Permissions.ProductsWrite));
            admin.Grant(permissions.Single(p => p.Name == ShopSphere.Domain.Users.Permissions.ProductsRead));
            admin.Grant(permissions.Single(p => p.Name == ShopSphere.Domain.Users.Permissions.OrdersReadAll));
            admin.Grant(permissions.Single(p => p.Name == ShopSphere.Domain.Users.Permissions.OrdersManage));

            Role customer = Role.Create("customer");
            customer.Grant(permissions.Single(p => p.Name == ShopSphere.Domain.Users.Permissions.ProductsRead));
            customer.Grant(permissions.Single(p => p.Name == ShopSphere.Domain.Users.Permissions.OrdersReadSelf));

            db.Roles.AddRange(admin, customer);

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Seeded {PermissionCount} permissions and {RoleCount} roles.",
                permissions.Length,
                2);
        }
        else
        {
            logger.LogInformation("Permissions already exist. Skipping.");
        }

        // ---------------------------------------------------------------------
        // Seed Catalog
        // ---------------------------------------------------------------------
        if (!await db.Categories.AnyAsync(ct))
        {
            logger.LogInformation("Seeding catalog data...");

            Category electronics = Category.Create("Electronics");
            Category books = Category.Create("Books");
            Category clothing = Category.Create("Clothing");

            db.Categories.AddRange(electronics, books, clothing);

            const string gbp = "GBP";

            Product[] products =
            [
                Product.Create(
                    "Wireless Headphones",
                    "Over-ear, 40h battery.",
                    Sku.From("ELEC-HDPH-001"),
                    electronics.Id,
                    new Money(129.99m, gbp)),

                Product.Create(
                    "Bluetooth Speaker",
                    "Portable, waterproof.",
                    Sku.From("ELEC-SPKR-001"),
                    electronics.Id,
                    new Money(59.50m, gbp)),

                Product.Create(
                    "USB-C Cable 2m",
                    "Braided, 100W PD.",
                    Sku.From("ELEC-CBL-002"),
                    electronics.Id,
                    new Money(9.99m, gbp)),

                Product.Create(
                    "Domain-Driven Design",
                    "Blue book. Evans, 2003.",
                    Sku.From("BOOK-DDD-001"),
                    books.Id,
                    new Money(45.00m, gbp)),

                Product.Create(
                    "Clean Architecture",
                    "Martin, 2017.",
                    Sku.From("BOOK-CLNARCH-001"),
                    books.Id,
                    new Money(38.00m, gbp)),

                Product.Create(
                    "Merino Wool T-Shirt",
                    "Grey, size M.",
                    Sku.From("CLTH-TSHRT-M-001"),
                    clothing.Id,
                    new Money(48.00m, gbp)),

                Product.Create(
                    "Waterproof Jacket",
                    "3-layer, size L.",
                    Sku.From("CLTH-JKT-L-001"),
                    clothing.Id,
                    new Money(180.00m, gbp))
            ];

            foreach (Product product in products)
            {
                product.Publish();
            }

            db.Products.AddRange(products);

            foreach (Product product in products)
            {
                StockLevel stock = StockLevel.Create(
                    product.Id,
                    initialAvailable: Random.Shared.Next(5, 40));

                db.StockLevels.Add(stock);
            }

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Seeded {CategoryCount} categories, {ProductCount} products, {StockCount} stock rows.",
                3,
                products.Length,
                products.Length);

            foreach (Product product in products)
            {
                logger.LogInformation(
                    "Seeded product {Sku} ('{Title}') priced {Price} - status {Status}",
                    product.Sku.Value,
                    product.Title,
                    product.Price.ToString(),
                    product.Status);
            }
        }
        else
        {
            logger.LogInformation("Catalog already exists. Skipping.");
        }
    }
}