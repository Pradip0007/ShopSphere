using MassTransit;
using ShopSphere.Api.Features.Inventory;
using ShopSphere.Api.Infrastructure.Messaging;
using ShopSphere.Contracts.Events;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Consumers;

public sealed class PaymentFailedCompensationConsumer(
    IInventoryClient inventory,
    IOrderRepository orders,
    IProcessedMessageStore processed,
    ILogger<PaymentFailedCompensationConsumer> logger)
    : IConsumer<PaymentFailed>
{
    private const string ConsumerName = nameof(PaymentFailedCompensationConsumer);

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var messageId = context.MessageId
            ?? throw new InvalidOperationException("Missing MessageId.");

        if (!await processed.TryMarkAsync(
                messageId,
                ConsumerName,
                context.CancellationToken))
        {
            return;
        }

        var order = await orders.FindAsync(
            new OrderId(context.Message.OrderId),
            context.CancellationToken);

        if (order is null)
        {
            logger.LogWarning(
                "Order not found for payment failure orderId={OrderId}",
                context.Message.OrderId);
            return;
        }

        foreach (var line in order.Items)
        {
            await inventory.ReleaseAsync(
                order.Id.Value,
                line.Sku,
                line.Quantity,
                context.CancellationToken);
        }

        order.MarkPaymentFailed();
        await orders.SaveChangesAsync(context.CancellationToken);
    }
}
