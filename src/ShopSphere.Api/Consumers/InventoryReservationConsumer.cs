using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Contracts.Events;
using ShopSphere.Api.Infrastructure.Messaging;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Ordering;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Consumers;

public sealed class InventoryReservationConsumer(
    ShopSphereDbContext db,
    IOrderRepository orders,
    IProcessedMessageStore processed,
    ILogger<InventoryReservationConsumer> logger)
    : IConsumer<OrderPlaced>
{
    private const string ConsumerName = nameof(InventoryReservationConsumer);

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var messageId = context.MessageId
            ?? throw new InvalidOperationException("Missing MessageId — is MassTransit configured correctly?");

        if (!await processed.TryMarkAsync(messageId, ConsumerName, context.CancellationToken))
        {
            logger.LogInformation("Skipping duplicate OrderPlaced messageId={MessageId}", messageId);
            return;
        }

        var msg = context.Message;
        var reservedSoFar = new List<(Domain.Inventory.StockLevel Stock, int Qty)>();
        var failures = new List<InventoryLineFailure>();

        // Reserve each line via the real domain aggregate.
        foreach (var line in msg.Lines)
        {
            var pid = new ProductId(line.ProductId);
            var stock = await db.StockLevels.SingleOrDefaultAsync(s => s.ProductId == pid, context.CancellationToken);
            if (stock is null)
            {
                failures.Add(new InventoryLineFailure(line.ProductId, line.Sku, line.Quantity, 0));
                continue;
            }

            var result = stock.Reserve(line.Quantity);
            if (result.IsFailure)
            {
                failures.Add(new InventoryLineFailure(line.ProductId, stock.Sku.Value, line.Quantity, stock.Available));
            }
            else
            {
                reservedSoFar.Add((stock, line.Quantity));
            }
        }

        if (failures.Count > 0)
        {
            // Compensate: release everything we reserved so far in this consume.
            // (Day 47 also handles cross-consume compensation via a saga.)
            foreach (var (stock, qty) in reservedSoFar)
                stock.Release(qty);

            logger.LogWarning(
                "Inventory reservation failed for orderId={OrderId} failures={FailureCount}",
                msg.OrderId, failures.Count);

            await context.Publish(new InventoryReservationFailed(
                msg.OrderId,
                Reason: "One or more lines had insufficient stock.",
                Failures: failures,
                FailedAtUtc: DateTimeOffset.UtcNow),
                context.CancellationToken);
            return;
        }

        // Transition the persisted Order to InventoryReserved.
        var order = await orders.FindAsync(new OrderId(msg.OrderId), context.CancellationToken);
        if (order is null)
        {
            logger.LogError("Order {OrderId} vanished between publish and consume — this should never happen.", msg.OrderId);
            return;
        }
        order.MarkInventoryReserved();

        await db.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Inventory reserved for orderId={OrderId} lineCount={LineCount}",
            msg.OrderId, msg.Lines.Count);

        await context.Publish(new InventoryReserved(msg.OrderId, DateTimeOffset.UtcNow),
            context.CancellationToken);
    }
}