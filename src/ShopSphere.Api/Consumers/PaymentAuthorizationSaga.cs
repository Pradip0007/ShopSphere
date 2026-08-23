using MassTransit;
using ShopSphere.Api.Contracts.Events;
using ShopSphere.Api.Infrastructure.Messaging;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Ordering;
using ShopSphere.Domain.Payments;

namespace ShopSphere.Api.Consumers;

public sealed class PaymentAuthorizationSaga : IConsumer<InventoryReserved>
{
    private const string ConsumerName = nameof(PaymentAuthorizationSaga);

    private readonly IOrderRepository _orders;
    private readonly IPaymentGateway _gateway;
    private readonly IProcessedMessageStore _processed;
    private readonly ILogger<PaymentAuthorizationSaga> _logger;

    public PaymentAuthorizationSaga(
        IOrderRepository orders,
        IPaymentGateway gateway,
        IProcessedMessageStore processed,
        ILogger<PaymentAuthorizationSaga> logger)
    {
        _orders = orders;
        _gateway = gateway;
        _processed = processed;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReserved> context)
    {
        var messageId = context.MessageId
            ?? throw new InvalidOperationException("Missing MessageId.");

        if (!await _processed.TryMarkAsync(messageId, ConsumerName, context.CancellationToken))
        {
            _logger.LogInformation("Skipping duplicate InventoryReserved messageId={MessageId}", messageId);
            return;
        }

        var order = await _orders.FindAsync(new OrderId(context.Message.OrderId), context.CancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Order not found for InventoryReserved orderId={OrderId}", context.Message.OrderId);
            return;
        }

        // For demo purposes the payment method comes from a test fixture — Day 41
        // hooks the real customer-provided payment method id from the checkout DTO.
        const string TestPaymentMethod = "pm_card_visa";

        var result = await _gateway.AuthorizeAsync(
            order.Subtotal,
            TestPaymentMethod,
            idempotencyKey: $"authorize:{order.Id.Value:D}",
            context.CancellationToken);

        if (!result.Succeeded)
        {
            _logger.LogWarning(
                "Payment authorization failed | orderId={OrderId} reason={Reason}",
                order.Id.Value, result.DeclineReason);

            await context.Publish(new PaymentFailed(
                order.Id.Value,
                result.DeclineReason ?? "Unknown decline",
                DateTimeOffset.UtcNow), context.CancellationToken);
            return;
        }

        _logger.LogInformation(
            "Payment authorized | orderId={OrderId} paymentIntent={PI}",
            order.Id.Value, result.PaymentIntentId);

        await context.Publish(new PaymentAuthorized(
            order.Id.Value,
            result.PaymentIntentId!,
            order.Subtotal.Amount,
            order.Subtotal.Currency,
            DateTimeOffset.UtcNow), context.CancellationToken);
    }
}