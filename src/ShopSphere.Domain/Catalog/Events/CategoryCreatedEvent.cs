using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Catalog.Events;

public sealed record CategoryCreatedEvent(
    CategoryId CategoryId,
    string Name,
    Slug Slug,
    CategoryId? ParentId) : DomainEvent;