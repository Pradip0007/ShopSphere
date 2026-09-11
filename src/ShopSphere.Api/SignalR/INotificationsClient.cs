namespace ShopSphere.Api.SignalR;

/// <summary>
/// Methods the server can invoke on the connected browser client.
/// Every method name here corresponds 1-to-1 to a <c>connection.on("xxx", ...)</c>
/// registration on the React side.
/// </summary>
public interface INotificationsClient
{
    Task StockChanged(StockChangedEvent evt);
    Task OrderStatusChanged(OrderStatusChangedEvent evt);
}

public sealed record StockChangedEvent(
    string Sku,
    int Available,
    DateTimeOffset Timestamp);

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    string Status,
    DateTimeOffset Timestamp);
