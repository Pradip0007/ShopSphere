using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.UnitTests.Ordering;

public sealed class OrderItemTests
{
    private static OrderItem NewItem(
        decimal price = 10m,
        int quantity = 2) =>
        new(
            productId: ProductId.New(),
            sku: "TEST-SKU",
            productNameSnapshot: "Test Product",
            unitPriceSnapshot: new Money(price, "USD"),
            quantity: quantity);

    [Fact]
    public void Constructor_should_set_properties()
    {
        var productId = ProductId.New();
        var price = new Money(25m, "USD");

        var item = new OrderItem(
            productId,
            "SKU-123",
            "Product Name",
            price,
            3);

        item.ProductId.Should().Be(productId);
        item.Sku.Should().Be("SKU-123");
        item.ProductNameSnapshot.Should().Be("Product Name");
        item.UnitPriceSnapshot.Should().Be(price);
        item.Quantity.Should().Be(3);
        item.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void LineTotal_should_multiply_unit_price_by_quantity()
    {
        var item = NewItem(price: 12.50m, quantity: 4);

        item.LineTotal.Should().Be(new Money(50m, "USD"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_should_reject_empty_sku(string sku)
    {
        var act = () => new OrderItem(
            ProductId.New(),
            sku,
            "Product",
            new Money(10m, "USD"),
            1);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*SKU required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_should_reject_empty_product_name(string name)
    {
        var act = () => new OrderItem(
            ProductId.New(),
            "SKU-123",
            name,
            new Money(10m, "USD"),
            1);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Name required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_should_reject_non_positive_quantity(int quantity)
    {
        var act = () => NewItem(quantity: quantity);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Quantity must be positive*");
    }
}