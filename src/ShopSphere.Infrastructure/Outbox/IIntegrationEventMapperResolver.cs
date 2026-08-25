using ShopSphere.Domain.Common;

namespace ShopSphere.Infrastructure.Outbox;

public interface IIntegrationEventMapperResolver
{
    object? Map(IDomainEvent domainEvent);
}