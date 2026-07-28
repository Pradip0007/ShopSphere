using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Inventory.Events;

public sealed record StockReleasedEvent(
    StockLevelId StockLevelId,
    ProductId ProductId,
    int Quantity,
    int AvailableAfter,
    int ReservedAfter) : DomainEvent;