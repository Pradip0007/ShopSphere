using ShopSphere.Domain.Catalog.Events;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Catalog;

/// <summary>
/// Throwaway aggregate — exists only to prove Raise/DomainEvents/Clear compose.
/// Delete when the Product aggregate is introduced on Day 6.
/// </summary>
public sealed class PingAggregate : AggregateRoot<Guid>
{
    private PingAggregate(Guid id) : base(id) { }

    public static PingAggregate Create(string message)
    {
        var agg = new PingAggregate(Guid.NewGuid());
        agg.Raise(new PingEvent(message));
        return agg;
    }
}