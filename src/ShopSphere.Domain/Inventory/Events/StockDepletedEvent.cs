using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Inventory.Events;

/// <summary>
/// Fired the moment Available hits zero. Merchandising will listen for this to hide the product.
/// </summary>
public sealed record StockDepletedEvent(
    StockLevelId StockLevelId,
    ProductId ProductId) : DomainEvent;