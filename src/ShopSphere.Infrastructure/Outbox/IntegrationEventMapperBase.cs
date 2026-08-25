using ShopSphere.Domain.Common;

namespace ShopSphere.Infrastructure.Outbox;

public abstract class IntegrationEventMapperBase<TDomain, TIntegration>
    : IIntegrationEventMapper<TDomain, TIntegration>, IIntegrationEventMarker
    where TDomain : IDomainEvent
    where TIntegration : notnull
{
    public Type DomainType => typeof(TDomain);

    public abstract TIntegration Map(TDomain domainEvent);

    public object MapObject(IDomainEvent domainEvent)
        => Map((TDomain)domainEvent);
}