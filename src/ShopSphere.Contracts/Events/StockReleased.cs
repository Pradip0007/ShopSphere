namespace ShopSphere.Contracts.Events;

public sealed record StockReleased(
    Guid OrderId,
    string Sku,
    int Quantity,
    int Remaining,
    DateTimeOffset ReleasedAtUtc);