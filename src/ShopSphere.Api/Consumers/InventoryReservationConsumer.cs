using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ShopSphere.Api.Features.Inventory;
using ShopSphere.Api.Infrastructure.Messaging;
using ShopSphere.Api.SignalR;
using ShopSphere.Contracts.Events;
using ShopSphere.Domain.Ordering;
using ShopSphere.Api.Features.Orders.OrderBrodcast;

namespace ShopSphere.Api.Consumers;

public sealed class InventoryReservationConsumer(
    IInventoryClient inventory,
    IOrderRepository orders,
    IProcessedMessageStore processed,
    IHubContext<NotificationsHub, INotificationsClient> hub,
    IOrderStatusBroadcaster broadcaster,
    ILogger<InventoryReservationConsumer> logger)
    : IConsumer<OrderPlaced>
{
    private const string ConsumerName = nameof(InventoryReservationConsumer);

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var messageId = context.MessageId
            ?? throw new InvalidOperationException(
                "Missing MessageId — is MassTransit configured correctly?");

        if (!await processed.TryMarkAsync(
                messageId,
                ConsumerName,
                context.CancellationToken))
        {
            logger.LogInformation(
                "Skipping duplicate OrderPlaced messageId={MessageId}",
                messageId);

            return;
        }

        var msg = context.Message;

        var reservedLines =
            new List<(string Sku, int Quantity, int AvailableAfter)>();

        var failures =
            new List<InventoryLineFailure>();

        foreach (var line in msg.Lines)
        {
            var result = await inventory.ReserveAsync(
                msg.OrderId,
                line.Sku,
                line.Quantity,
                context.CancellationToken);

            if (!result.Success)
            {
                failures.Add(
                    new InventoryLineFailure(
                        line.ProductId,
                        line.Sku,
                        line.Quantity,
                        result.AvailableAfter));

                continue;
            }

            reservedLines.Add(
                (line.Sku, line.Quantity, result.AvailableAfter));
        }

        if (failures.Count > 0)
        {
            foreach (var reserved in reservedLines)
            {
                await inventory.ReleaseAsync(
                    msg.OrderId,
                    reserved.Sku,
                    reserved.Quantity,
                    context.CancellationToken);
            }

            logger.LogWarning(
                "Inventory reservation failed for orderId={OrderId} failures={FailureCount}",
                msg.OrderId,
                failures.Count);

            await context.Publish(
                new InventoryReservationFailed(
                    msg.OrderId,
                    Reason: "One or more lines had insufficient stock.",
                    Failures: failures,
                    FailedAtUtc: DateTimeOffset.UtcNow),
                context.CancellationToken);

            return;
        }

        var order = await orders.FindAsync(
            new OrderId(msg.OrderId),
            context.CancellationToken);

        if (order is null)
        {
            logger.LogError(
                "Order {OrderId} vanished between publish and consume — this should never happen.",
                msg.OrderId);

            return;
        }

        order.MarkInventoryReserved();

        await orders.SaveChangesAsync(
        context.CancellationToken);

        await broadcaster.BroadcastAsync(
            order.Id.Value,
            OrderStatus.InventoryReserved,
            context.CancellationToken);

        foreach (var reserved in reservedLines)
        {
            var evt = new StockChangedEvent(
                reserved.Sku,
                reserved.AvailableAfter,
                DateTimeOffset.UtcNow);

            await hub
                .Clients
                .Group(GroupName.Stock(reserved.Sku))
                .StockChanged(evt);
        }

        logger.LogInformation(
            "Inventory reserved for orderId={OrderId} lineCount={LineCount}",
            msg.OrderId,
            msg.Lines.Count);

        await context.Publish(
            new InventoryReserved(
                msg.OrderId,
                DateTimeOffset.UtcNow),
            context.CancellationToken);
    }
}