namespace ShopSphere.Domain.Common;

/// <summary>
/// Marker + hook base for aggregate roots.
/// Day 4 will add domain event support here.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }
}