using ShopSphere.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace ShopSphere.Infrastructure.Outbox;

public sealed class IntegrationEventMapperResolver(
    IServiceProvider services) : IIntegrationEventMapperResolver
{
    public object? Map(IDomainEvent domainEvent)
    {
        var markers = services.GetServices<IIntegrationEventMarker>();

        foreach (var marker in markers)
        {
            if (marker.DomainType == domainEvent.GetType())
            {
                return marker.MapObject(domainEvent);
            }
        }

        return null;
    }
}