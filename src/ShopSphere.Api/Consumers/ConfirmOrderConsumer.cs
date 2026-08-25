using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Contracts.Events;
using ShopSphere.Api.Infrastructure.Messaging;
using ShopSphere.Domain.Ordering;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Consumers;

public sealed class ConfirmOrderConsumer : IConsumer<PaymentAuthorized>
{
    private const string ConsumerName = nameof(ConfirmOrderConsumer);

    private readonly IOrderRepository _orders;
    private readonly ShopSphereDbContext _db;
    private readonly IProcessedMessageStore _processed;
    private readonly ILogger<ConfirmOrderConsumer> _logger;

    public ConfirmOrderConsumer(
        IOrderRepository orders,
        ShopSphereDbContext db,
        IProcessedMessageStore processed,
        ILogger<ConfirmOrderConsumer> logger)
    {
        _orders = orders;
        _db = db;
        _processed = processed;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        var messageId = context.MessageId
            ?? throw new InvalidOperationException("Missing MessageId.");

        // Prevent the same PaymentAuthorized message from
        // confirming the order and sending the email twice.
        if (!await _processed.TryMarkAsync(
                messageId,
                ConsumerName,
                context.CancellationToken))
        {
            _logger.LogInformation(
                "Skipping duplicate PaymentAuthorized messageId={MessageId}",
                messageId);

            return;
        }

        // 1. Load the order.
        var order = await _orders.FindAsync(
            new OrderId(context.Message.OrderId),
            context.CancellationToken);

        if (order is null)
        {
            _logger.LogWarning(
                "Order not found for PaymentAuthorized orderId={OrderId}",
                context.Message.OrderId);

            return;
        }

        // 2. Move the order through the expected state transitions.
        //
        // Inventory was already reserved by InventoryReservationConsumer.
        // Therefore, we must NOT call MarkInventoryReserved() again.
        order.MarkPaymentAuthorized();
        order.MarkConfirmed();

        await _orders.SaveChangesAsync(context.CancellationToken);

        // 3. Find the customer directly from EF.
        //
        // User.Id is a strongly typed UserId while Order.UserId is a Guid.
        // Convert the Guid to UserId so EF can use the configured
        // UserId -> Guid value converter.
        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                u => u.Id == new UserId(order.UserId),
                context.CancellationToken);

        if (user is null)
        {
            _logger.LogWarning(
                "User not found for orderId={OrderId} userId={UserId}",
                order.Id.Value,
                order.UserId);

            return;
        }

        // 4. Build the order lines for the notification.
        var lines = order.Items
            .Select(item => new OrderConfirmedLine(
                item.Sku,
                item.ProductNameSnapshot,
                item.UnitPriceSnapshot.Amount,
                item.Quantity))
            .ToArray();

        // 5. Publish OrderConfirmed.
        //
        // OrderConfirmedEmailConsumer will receive this event
        // and send the confirmation email through IEmailSender.
        await context.Publish(
            new OrderConfirmed(
                order.Id.Value,
                order.UserId,
                user.Email,
                order.Subtotal.Amount,
                order.Subtotal.Currency,
                DateTimeOffset.UtcNow,
                lines),
            context.CancellationToken);

        _logger.LogInformation(
            "Order confirmed | orderId={OrderId} email={Email}",
            order.Id.Value,
            user.Email);
    }
}