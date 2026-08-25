using MassTransit;
using ShopSphere.Contracts.Events;

namespace ShopSphere.Api.Consumers;

public sealed class PaymentFailedLogger : IConsumer<PaymentFailed>
{
    private readonly ILogger<PaymentFailedLogger> _logger;
    public PaymentFailedLogger(ILogger<PaymentFailedLogger> logger) => _logger = logger;

    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        _logger.LogWarning(
            "PaymentFailed observed | orderId={OrderId} reason={Reason}",
            context.Message.OrderId, context.Message.Reason);
        return Task.CompletedTask;
    }
}