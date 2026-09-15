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

    [Fact]
    public void Create_should_set_product_sku_and_available_counts()
    {
        var productId = ProductId.New();
        var sku = Sku.From("STOCK-1");

        var stock = StockLevel.Create(productId, sku, 4);

        stock.ProductId.Should().Be(productId);
        stock.Sku.Should().Be(sku);
        stock.Available.Should().Be(4);
        stock.Reserved.Should().Be(0);
        stock.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Create_should_reject_negative_initial_stock()
    {
        var act = () => StockLevel.Create(ProductId.New(), Sku.From("STOCK-1"), -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_should_reject_null_sku()
    {
        var act = () => StockLevel.Create(ProductId.New(), null!, 0);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reserve_should_reject_non_positive_quantity(int quantity)
    {
        var result = NewStock().Reserve(quantity);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("inventory.quantity_not_positive");
    }

    [Fact]
    public void Reserve_should_raise_event_with_updated_counts()
    {
        var stock = NewStock(10);

        stock.Reserve(3);

        var @event = stock.DomainEvents.Should().ContainSingle().Which.Should()
            .BeOfType<StockReservedEvent>().Subject;

        @event.StockLevelId.Should().Be(stock.Id);
        @event.ProductId.Should().Be(stock.ProductId);
        @event.Quantity.Should().Be(3);
        @event.AvailableAfter.Should().Be(7);
        @event.ReservedAfter.Should().Be(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Release_should_reject_non_positive_quantity(int quantity)
    {
        var result = NewStock().Release(quantity);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("inventory.quantity_not_positive");
    }

    [Fact]
    public void Release_should_raise_event_with_updated_counts()
    {
        var stock = NewStock(10);
        stock.Reserve(4);
        stock.ClearDomainEvents();

        stock.Release(2);

        var @event = stock.DomainEvents.Should().ContainSingle().Which.Should()
            .BeOfType<StockReleasedEvent>().Subject;

        @event.StockLevelId.Should().Be(stock.Id);
        @event.ProductId.Should().Be(stock.ProductId);
        @event.Quantity.Should().Be(2);
        @event.AvailableAfter.Should().Be(8);
        @event.ReservedAfter.Should().Be(2);
    }

    [Fact]
    public void Adjust_to_zero_from_positive_should_raise_depleted_event()
    {
        var stock = NewStock(5);

        stock.Adjust(-5);

        stock.DomainEvents.Should().ContainSingle(e => e is StockDepletedEvent);
    }

    [Fact]
    public void Adjust_when_already_zero_should_not_raise_depleted_event()
    {
        var stock = NewStock(0);

        stock.Adjust(0);

        stock.DomainEvents.Should().BeEmpty();
    }
}