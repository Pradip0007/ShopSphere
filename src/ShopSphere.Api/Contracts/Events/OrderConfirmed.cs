namespace ShopSphere.Api.Contracts.Events;

public sealed record OrderConfirmed(
    Guid OrderId,
    Guid UserId,
    string CustomerEmail,
    decimal Total,
    string Currency,
    DateTimeOffset ConfirmedAtUtc,
    IReadOnlyList<OrderConfirmedLine> Lines);

public sealed record OrderConfirmedLine(
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int Quantity);