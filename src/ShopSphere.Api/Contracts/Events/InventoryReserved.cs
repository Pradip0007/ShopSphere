namespace ShopSphere.Api.Contracts.Events;

public sealed record InventoryReserved(
    Guid OrderId,
    DateTimeOffset ReservedAtUtc);