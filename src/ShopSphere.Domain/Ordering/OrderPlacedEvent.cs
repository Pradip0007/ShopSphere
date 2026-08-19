using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Ordering;

public sealed record OrderPlacedEvent(
    OrderId OrderId,
    Guid UserId,
    Money Total,
    int LineCount,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}