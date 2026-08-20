using MassTransit;
using ShopSphere.Api.Contracts.Events;

namespace ShopSphere.Api.Consumers;

public sealed class OrderPlacedLogger : IConsumer<OrderPlaced>
{
    private readonly ILogger<OrderPlacedLogger> _logger;

    public OrderPlacedLogger(ILogger<OrderPlacedLogger> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var m = context.Message;
        _logger.LogInformation(
            "OrderPlaced received | orderId={OrderId} userId={UserId} total={Total} {Currency} lines={LineCount} messageId={MessageId}",
            m.OrderId, m.UserId, m.Total, m.Currency, m.Lines.Count, context.MessageId);
        return Task.CompletedTask;
    }
}