using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

namespace ShopSphere.UnitTests;

public static class TestBuilders
{
    public static Product Draft(
        string skuValue = "TEST-SKU-1",
        string title = "Test Product",
        decimal price = 9.99m)
        => Product.Create(
            title: title,
            description: "Test description",
            sku: Sku.From(skuValue),
            categoryId: CategoryId.New(),
            price: new Money(price, "USD"));
}