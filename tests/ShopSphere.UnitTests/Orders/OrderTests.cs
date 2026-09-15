using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.UnitTests.Orders;

public sealed class OrderTests
{
    private static Address NewAddress() =>
        new(
            "123 Test Street",
            "Test City",
            "WB",
            "700001",
            "IN");

    private static OrderItem NewItem(
        decimal price = 10m,
        int quantity = 1,
        string currency = "USD") =>
        new(
            productId: ProductId.New(),
            sku: "TEST-SKU",
            productNameSnapshot: "Test Product",
            unitPriceSnapshot: new Money(price, currency),
            quantity: quantity);

    [Fact]
    public void Place_should_create_pending_order()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().ContainSingle();
        order.Currency.Should().Be("USD");
    }

    [Fact]
    public void Place_should_calculate_subtotal_from_line_totals()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [
                NewItem(price: 10m, quantity: 2),
                NewItem(price: 7.5m, quantity: 3)
            ],
            NewAddress());

        order.Subtotal.Amount.Should().Be(42.5m);
        order.Subtotal.Currency.Should().Be("USD");
    }

    [Fact]
    public void Place_should_raise_order_placed_event()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        order.DomainEvents.Should()
            .ContainSingle(e => e is OrderPlacedEvent);
    }

    [Fact]
    public void Place_should_reject_empty_lines()
    {
        var act = () => Order.Place(
            Guid.NewGuid(),
            [],
            NewAddress());

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*empty order*");
    }

    [Fact]
    public void Place_should_reject_mixed_currencies()
    {
        var act = () => Order.Place(
            Guid.NewGuid(),
            [
                NewItem(currency: "USD"),
                NewItem(currency: "EUR")
            ],
            NewAddress());

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*share a currency*");
    }

    [Fact]
    public void MarkInventoryReserved_should_change_status()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        order.MarkInventoryReserved();

        order.Status.Should().Be(OrderStatus.InventoryReserved);
    }

    [Fact]
    public void MarkPaymentAuthorized_should_change_status()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        order.MarkInventoryReserved();
        order.MarkPaymentAuthorized();

        order.Status.Should().Be(OrderStatus.PaymentAuthorized);
    }

    [Fact]
    public void MarkConfirmed_should_change_status()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        order.MarkInventoryReserved();
        order.MarkPaymentAuthorized();
        order.MarkConfirmed();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Cancel_should_change_pending_order_to_cancelled()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void MarkInventoryReserved_should_reject_invalid_state()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        order.Cancel();

        var act = () => order.MarkInventoryReserved();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkPaymentAuthorized_should_reject_pending_order()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        var act = () => order.MarkPaymentAuthorized();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkConfirmed_should_reject_pending_order()
    {
        var order = Order.Place(
            Guid.NewGuid(),
            [NewItem()],
            NewAddress());

        var act = () => order.MarkConfirmed();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Place_should_reject_empty_user_id_null_lines_and_null_address()
    {
        var item = NewItem();

        var emptyUser = () => Order.Place(Guid.Empty, [item], NewAddress());
        var nullLines = () => Order.Place(Guid.NewGuid(), null!, NewAddress());
        var nullAddress = () => Order.Place(Guid.NewGuid(), [item], null!);

        emptyUser.Should().Throw<ArgumentOutOfRangeException>();
        nullLines.Should().Throw<ArgumentNullException>();
        nullAddress.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Order_should_reject_reserving_inventory_after_cancellation()
    {
        var order = Order.Place(Guid.NewGuid(), [NewItem()], NewAddress());
        order.Cancel();

        var act = () => order.MarkInventoryReserved();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Order_should_reject_confirming_before_payment_authorization()
    {
        var order = Order.Place(Guid.NewGuid(), [NewItem()], NewAddress());
        order.MarkInventoryReserved();

        var act = () => order.MarkConfirmed();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OrderPlaced_event_should_reference_order_and_placement_time()
    {
        var order = Order.Place(Guid.NewGuid(), [NewItem()], NewAddress());

        var @event = order.DomainEvents.Single() as OrderPlacedEvent;

        @event.Should().NotBeNull();
        @event!.Order.Should().BeSameAs(order);
        @event.OccurredAt.Should().Be(order.PlacedAtUtc);
        @event.EventId.Should().NotBe(Guid.Empty);
    }
}