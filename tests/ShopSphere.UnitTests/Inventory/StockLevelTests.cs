using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Inventory;
using ShopSphere.Domain.Inventory.Events;

namespace ShopSphere.UnitTests.Inventory;

public sealed class StockLevelTests
{
    private static StockLevel NewStock(int qty = 10) =>
        StockLevel.Create(
            productId: ProductId.New(),
            sku: Sku.From("TEST-SKU-1"),
            initialAvailable: qty);

    [Fact]
    public void Reserve_should_decrement_available()
    {
        var stock = NewStock(10);

        var result = stock.Reserve(3);

        result.IsSuccess.Should().BeTrue();
        stock.Available.Should().Be(7);
        stock.Reserved.Should().Be(3);
    }

    [Fact]
    public void Reserve_should_fail_when_insufficient()
    {
        var stock = NewStock(5);

        var result = stock.Reserve(6);

        result.IsFailure.Should().BeTrue();
        stock.Available.Should().Be(5);
        stock.Reserved.Should().Be(0);
    }

    [Fact]
    public void Release_should_return_reservation_to_available()
    {
        var stock = NewStock(10);

        stock.Reserve(4);
        var result = stock.Release(4);

        result.IsSuccess.Should().BeTrue();
        stock.Available.Should().Be(10);
        stock.Reserved.Should().Be(0);
    }

    [Fact]
    public void Release_more_than_reserved_should_fail()
    {
        var stock = NewStock(10);

        stock.Reserve(2);
        var result = stock.Release(3);

        result.IsFailure.Should().BeTrue();
        stock.Available.Should().Be(8);
        stock.Reserved.Should().Be(2);
    }

    [Fact]
    public void Adjust_positive_should_increase_available()
    {
        var stock = NewStock(10);

        var result = stock.Adjust(5);

        result.IsSuccess.Should().BeTrue();
        stock.Available.Should().Be(15);
    }

    [Fact]
    public void Adjust_negative_should_decrease_available()
    {
        var stock = NewStock(10);

        var result = stock.Adjust(-3);

        result.IsSuccess.Should().BeTrue();
        stock.Available.Should().Be(7);
    }

    [Fact]
    public void Adjust_below_zero_should_fail()
    {
        var stock = NewStock(2);

        var result = stock.Adjust(-5);

        result.IsFailure.Should().BeTrue();
        stock.Available.Should().Be(2);
    }

    [Fact]
    public void Reserve_exact_available_should_raise_depleted_event()
    {
        var stock = NewStock(3);

        var result = stock.Reserve(3);

        result.IsSuccess.Should().BeTrue();
        stock.Available.Should().Be(0);
        stock.Reserved.Should().Be(3);
        stock.DomainEvents.Should()
            .ContainSingle(e => e is StockDepletedEvent);
    }
}