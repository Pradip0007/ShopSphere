using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Ordering;

public sealed class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderItem> _items = [];

    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; } = default!;
    public string Currency { get; private set; } = default!;
    public Money Subtotal { get; private set; } = default!;
    public DateTimeOffset PlacedAtUtc { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    // EF materialisation
    private Order() : base() { }

    private Order(OrderId id, Guid userId, Address shippingAddress, string currency)
        : base(id)
    {
        UserId = userId;
        ShippingAddress = shippingAddress;
        Currency = currency;
        Status = OrderStatus.Pending;
        Subtotal = new Money(0, currency);
        PlacedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Snapshots every line at current price. NEVER trust the cart at this point —
    /// the caller passes fully-hydrated OrderItem instances built from a live catalog read.
    /// </summary>
    public static Order Place(Guid userId, IReadOnlyList<OrderItem> lines, Address shippingAddress)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(shippingAddress);
        if (lines.Count == 0) throw new InvalidOperationException("Cannot place an empty order.");

        var currency = lines[0].UnitPriceSnapshot.Currency;
        if (lines.Any(l => l.UnitPriceSnapshot.Currency != currency))
        {
            throw new InvalidOperationException("All lines must share a currency.");
        }

        var order = new Order(OrderId.New(), userId, shippingAddress, currency);
        foreach (var line in lines)
        {
            order._items.Add(line);
        }

        order.Subtotal = lines.Aggregate(new Money(0, currency), (acc, l) => acc + l.LineTotal);
        order.Raise(new OrderPlacedEvent(order.Id, userId, order.Subtotal, order._items.Count, order.PlacedAtUtc));
        return order;
    }

    public void MarkInventoryReserved()
    {
        if (Status != OrderStatus.Pending) throw new InvalidOperationException($"Cannot reserve inventory in state {Status}.");
        Status = OrderStatus.InventoryReserved;
    }

    public void MarkPaymentAuthorized()
    {
        if (Status != OrderStatus.InventoryReserved) throw new InvalidOperationException($"Cannot authorize payment in state {Status}.");
        Status = OrderStatus.PaymentAuthorized;
    }

    public void MarkConfirmed()
    {
        if (Status != OrderStatus.PaymentAuthorized) throw new InvalidOperationException($"Cannot confirm in state {Status}.");
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
        {
            throw new InvalidOperationException($"Cannot cancel in state {Status}.");
        }
        Status = OrderStatus.Cancelled;
    }
}