namespace ShopSphere.Domain.Common;

/// <summary>
/// Convenience base for domain events. Auto-fills EventId and OccurredAt.
/// Prefer inheriting this over implementing IDomainEvent by hand.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}