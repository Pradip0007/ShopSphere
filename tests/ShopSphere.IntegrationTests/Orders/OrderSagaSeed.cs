using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.IntegrationTests.Orders;

internal static class OrderSagaSeed
{
    public static async Task<(Guid UserId, Guid ProductId)> SeedCustomerAndProduct(
        ShopSphereDbContext db,
        string sku,
        decimal price,
        int stockQuantity)
    {
        var user = User.Register(
            $"saga-{Guid.NewGuid():N}@example.com",
            "StrongPassword1",
            new TestPasswordHasher());
        var category = Category.Create($"Saga {Guid.NewGuid():N}");
        var product = Product.Create(
            "Saga Widget",
            "Widget for saga integration tests",
            Sku.From(sku),
            category.Id,
            new Money(price, "USD"));
        product.Publish();
        var stock = StockLevel.Create(product.Id, product.Sku, stockQuantity);

        db.Users.Add(user);
        db.Categories.Add(category);
        db.Products.Add(product);
        db.StockLevels.Add(stock);
        await db.SaveChangesAsync();

        return (user.Id.Value, product.Id.Value);
    }

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public string Hash(string plainPassword) => $"test:{plainPassword}";
        public bool Verify(string plainPassword, string storedHash) =>
            storedHash == $"test:{plainPassword}";
    }
}
