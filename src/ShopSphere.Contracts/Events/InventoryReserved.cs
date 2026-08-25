namespace ShopSphere.Contracts.Events;

public sealed record InventoryReserved(
    Guid OrderId,
    DateTimeOffset ReservedAtUtc);