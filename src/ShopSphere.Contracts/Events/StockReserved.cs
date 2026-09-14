namespace ShopSphere.Contracts.Events;

public sealed record StockReserved(
    Guid OrderId,
    string Sku,
    int Quantity,
    int Remaining,
    DateTimeOffset ReservedAtUtc);