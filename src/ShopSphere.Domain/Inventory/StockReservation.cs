namespace ShopSphere.Domain.Inventory;

public sealed class StockReservation
{
    public Guid OrderId { get; private set; }
    public string Sku { get; private set; } = null!;
    public int Quantity { get; private set; }
    public DateTimeOffset ReservedAt { get; private set; }

    private StockReservation() { }

    public StockReservation(
        Guid orderId,
        string sku,
        int quantity,
        DateTimeOffset reservedAt)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("orderId");

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("sku");

        if (quantity <= 0)
            throw new ArgumentException("quantity");

        OrderId = orderId;
        Sku = sku;
        Quantity = quantity;
        ReservedAt = reservedAt;
    }
}