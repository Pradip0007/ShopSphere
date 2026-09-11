using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ShopSphere.Api.SignalR;

[Authorize]
public sealed class NotificationsHub : Hub<INotificationsClient>
{
    private readonly ILogger<NotificationsHub> _log;

    public NotificationsHub(ILogger<NotificationsHub> log) => _log = log;

    public async Task JoinOrderGroup(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new HubException("orderId is required");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName.Order(orderId));
        _log.LogDebug(
            "{Conn} joined {Group}",
            Context.ConnectionId,
            GroupName.Order(orderId));
    }

    public Task LeaveOrderGroup(Guid orderId) =>
        Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GroupName.Order(orderId));

    public async Task JoinStockGroup(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new HubException("sku is required");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName.Stock(sku));
        _log.LogDebug(
            "{Conn} joined {Group}",
            Context.ConnectionId,
            GroupName.Stock(sku));
    }

    public Task LeaveStockGroup(string sku) =>
        Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GroupName.Stock(sku));

    public override Task OnConnectedAsync()
    {
        _log.LogInformation(
            "SignalR connected: {Conn} user={User}",
            Context.ConnectionId,
            Context.UserIdentifier);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _log.LogInformation(
            "SignalR disconnected: {Conn} ({Reason})",
            Context.ConnectionId,
            exception?.Message ?? "clean");

        return base.OnDisconnectedAsync(exception);
    }
}

public static class GroupName
{
    public static string Order(Guid orderId) => $"order:{orderId}";

    public static string Stock(string sku) => $"stock:{sku}";
}
