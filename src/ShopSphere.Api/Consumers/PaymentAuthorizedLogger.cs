using MassTransit;
using ShopSphere.Contracts.Events;

namespace ShopSphere.Api.Consumers;

public sealed class PaymentAuthorizedLogger : IConsumer<PaymentAuthorized>
{
    private readonly ILogger<PaymentAuthorizedLogger> _logger;
    public PaymentAuthorizedLogger(ILogger<PaymentAuthorizedLogger> logger) => _logger = logger;

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        _logger.LogInformation(
            "PaymentAuthorized observed | orderId={OrderId} paymentIntent={PI}",
            context.Message.OrderId, context.Message.PaymentIntentId);
        return Task.CompletedTask;
    }
}