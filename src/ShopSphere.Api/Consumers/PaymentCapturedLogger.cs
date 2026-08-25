using MassTransit;
using ShopSphere.Contracts.Events;

namespace ShopSphere.Api.Consumers;

public sealed class PaymentCapturedLogger : IConsumer<PaymentCaptured>
{
    private readonly ILogger<PaymentCapturedLogger> _logger;
    public PaymentCapturedLogger(ILogger<PaymentCapturedLogger> logger) => _logger = logger;

    public Task Consume(ConsumeContext<PaymentCaptured> context)
    {
        var m = context.Message;
        _logger.LogInformation(
            "PaymentCaptured observed | orderId={OrderId} amount={Amount} {Currency}",
            m.OrderId, m.Amount, m.Currency);
        return Task.CompletedTask;
    }
}