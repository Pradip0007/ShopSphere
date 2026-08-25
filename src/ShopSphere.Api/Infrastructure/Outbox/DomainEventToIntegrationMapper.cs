using ShopSphere.Api.Contracts.Events;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Infrastructure.Outbox;

public sealed class DomainEventToIntegrationMapper : IDomainEventToIntegrationMapper
{
    public object? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        OrderPlacedEvent op => new OrderPlaced(
            op.OrderId.Value,
            op.UserId,
            op.Total.Amount,
            op.Total.Currency,
            op.OccurredAt,
            Array.Empty<OrderPlacedLine>()),
        _ => null
    };
}
