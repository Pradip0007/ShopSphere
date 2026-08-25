using ShopSphere.Domain.Common;

namespace ShopSphere.Infrastructure.Outbox;

public interface IIntegrationEventMapper<in TDomain, out TIntegration>
    where TDomain : IDomainEvent
    where TIntegration : notnull
{
    TIntegration Map(TDomain domainEvent);
}