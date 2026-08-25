using ShopSphere.Domain.Common;

namespace ShopSphere.Api.Infrastructure.Outbox;

public interface IDomainEventToIntegrationMapper
{
    object? Map(IDomainEvent domainEvent);
}
