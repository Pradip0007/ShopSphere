namespace ShopSphere.Contracts.Events;

/// <summary>
/// Published when an order is placed. All types are primitive on purpose —
/// this contract is the API between Ordering and every downstream context
/// (Inventory, Payments, Notifications, Analytics).
/// </summary>
public sealed record OrderPlaced(
    Guid OrderId,
    Guid UserId,
    decimal Total,
    string Currency,
    DateTimeOffset PlacedAtUtc,
    IReadOnlyList<OrderPlacedLine> Lines);

public sealed record OrderPlacedLine(
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int Quantity);