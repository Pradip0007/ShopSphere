namespace ShopSphere.Domain.Common;

/// <summary>
/// Placeholder dispatcher — drops events on the floor.
/// Registered by default so wiring code compiles and tests pass
/// even before real handlers exist.
/// </summary>
public sealed class NullDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct = default)
        => Task.CompletedTask;
}