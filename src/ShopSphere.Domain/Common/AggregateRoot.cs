namespace ShopSphere.Domain.Common;

/// <summary>
/// Base for aggregate roots. Tracks unpublished domain events raised during
/// this unit of work. The persistence layer drains them after SaveChanges.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }
    /// <summary>
    /// Read-only view of events raised since the last ClearDomainEvents().
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Called from inside aggregate methods when something interesting happens.
    /// </summary>
    protected void Raise(IDomainEvent @event) => _domainEvents.Add(@event);

    /// <summary>
    /// Called by the persistence layer after events have been dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}