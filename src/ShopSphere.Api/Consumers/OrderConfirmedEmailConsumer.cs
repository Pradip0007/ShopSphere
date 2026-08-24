using System.Globalization;
using System.Text;
using MassTransit;
using ShopSphere.Api.Contracts.Events;
using ShopSphere.Api.Infrastructure.Messaging;
using ShopSphere.Domain.Notifications;

namespace ShopSphere.Api.Consumers;

public sealed class OrderConfirmedEmailConsumer : IConsumer<OrderConfirmed>
{
    private const string ConsumerName = nameof(OrderConfirmedEmailConsumer);

    private readonly IEmailSender _sender;
    private readonly IProcessedMessageStore _processed;
    private readonly ILogger<OrderConfirmedEmailConsumer> _logger;

    public OrderConfirmedEmailConsumer(
        IEmailSender sender,
        IProcessedMessageStore processed,
        ILogger<OrderConfirmedEmailConsumer> logger)
    {
        _sender = sender;
        _processed = processed;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var messageId = context.MessageId ?? throw new InvalidOperationException("Missing MessageId.");
        if (!await _processed.TryMarkAsync(messageId, ConsumerName, context.CancellationToken))
        {
            _logger.LogInformation("Skipping duplicate OrderConfirmed messageId={MessageId}", messageId);
            return;
        }

        var m = context.Message;

        var plain = new StringBuilder();
        plain.AppendLine("Thanks for your order at ShopSphere!");
        plain.AppendLine();
        plain.AppendLine($"Order: {m.OrderId:D}");
        plain.AppendLine($"Placed: {m.ConfirmedAtUtc:u}");
        plain.AppendLine();
        plain.AppendLine("Items");
        plain.AppendLine("-----");
        foreach (var line in m.Lines)
        {
            plain.AppendLine(CultureInfo.InvariantCulture,
                $"  {line.Sku} — {line.ProductName} x{line.Quantity}  {line.UnitPrice:F2} {m.Currency}");
        }
        plain.AppendLine();
        plain.AppendLine(CultureInfo.InvariantCulture, $"Total: {m.Total:F2} {m.Currency}");

        await _sender.SendAsync(new EmailMessage(
            ToAddress: m.CustomerEmail,
            ToName: m.CustomerEmail,
            Subject: $"Your ShopSphere order {m.OrderId:D}",
            PlainBody: plain.ToString(),
            HtmlBody: null),
            context.CancellationToken);
    }
}