namespace ShopSphere.Domain.Common;

/// <summary>
/// Fans a batch of domain events out to their handlers.
/// Real implementation lands on Day 21 with MediatR.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct = default);
}