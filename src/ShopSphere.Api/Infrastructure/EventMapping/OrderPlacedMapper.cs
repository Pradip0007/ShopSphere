using ShopSphere.Contracts.Events;
using ShopSphere.Domain.Ordering;
using ShopSphere.Infrastructure.Outbox;

namespace ShopSphere.Api.Infrastructure.EventMapping;

public sealed class OrderPlacedMapper
    : IntegrationEventMapperBase<OrderPlacedEvent, OrderPlaced>
{
    public override OrderPlaced Map(OrderPlacedEvent domainEvent)
    {
        var order = domainEvent.Order;

        var lines = order.Items
            .Select(i => new OrderPlacedLine(
                i.ProductId.Value,
                i.Sku,
                i.ProductNameSnapshot,
                i.UnitPriceSnapshot.Amount,
                i.Quantity))
            .ToArray();

        return new OrderPlaced(
            OrderId: order.Id.Value,
            UserId: order.UserId,
            Total: order.Subtotal.Amount,
            Currency: order.Subtotal.Currency,
            PlacedAtUtc: order.PlacedAtUtc,
            Lines: lines);
    }
}