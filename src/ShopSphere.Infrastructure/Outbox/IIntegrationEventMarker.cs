using ShopSphere.Domain.Common;

namespace ShopSphere.Infrastructure.Outbox;

public interface IIntegrationEventMarker
{
    Type DomainType { get; }

    object MapObject(IDomainEvent domainEvent);
}