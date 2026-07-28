using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Catalog.Events;

public sealed record ProductPublishedEvent(
    ProductId ProductId,
    Sku Sku,
    CategoryId CategoryId,
    Money Price) : DomainEvent;