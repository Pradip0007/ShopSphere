using Microsoft.AspNetCore.SignalR;
using ShopSphere.Api.SignalR;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Features.Orders.OrderBrodcast;

public interface IOrderStatusBroadcaster
{
    Task BroadcastAsync(
        Guid orderId,
        string status,
        CancellationToken ct = default);

    Task BroadcastAsync(
        Guid orderId,
        OrderStatus status,
        CancellationToken ct = default);
}

public sealed class OrderStatusBroadcaster(
    IHubContext<NotificationsHub, INotificationsClient> hub,
    TimeProvider clock,
    ILogger<OrderStatusBroadcaster> logger)
    : IOrderStatusBroadcaster
{
    public Task BroadcastAsync(
    Guid orderId,
    OrderStatus status,
    CancellationToken ct = default) =>
    BroadcastAsync(orderId, status.ToString(), ct);

public async Task BroadcastAsync(
    Guid orderId,
    string status,
    CancellationToken ct = default)
{
    var evt = new OrderStatusChangedEvent(
        orderId,
        status,
        clock.GetUtcNow());

    await Task.WhenAll(
        hub.Clients
            .Group(GroupName.Order(orderId))
            .OrderStatusChanged(evt),
        hub.Clients
            .Group(GroupName.AdminOrders)
            .OrderStatusChanged(evt));

    logger.LogInformation(
        "Broadcast order {OrderId} -> {Status}",
        orderId,
        status);
}
}
