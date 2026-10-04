using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Ordering;

public sealed record OrderPlacedEvent(
    Order Order,
    string? PaymentMethodId,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}