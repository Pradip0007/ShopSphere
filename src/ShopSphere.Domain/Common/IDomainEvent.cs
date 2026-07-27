namespace ShopSphere.Domain.Common;

/// <summary>
/// Marker interface for something that happened in the domain.
/// Implementations should be immutable records — the past cannot change.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}