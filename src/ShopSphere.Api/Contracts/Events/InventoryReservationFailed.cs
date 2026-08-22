namespace ShopSphere.Api.Contracts.Events;

public sealed record InventoryReservationFailed(
    Guid OrderId,
    string Reason,
    IReadOnlyList<InventoryLineFailure> Failures,
    DateTimeOffset FailedAtUtc);

public sealed record InventoryLineFailure(
    Guid ProductId,
    string Sku,
    int RequestedQuantity,
    int AvailableQuantity);