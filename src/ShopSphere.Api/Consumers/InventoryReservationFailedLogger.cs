using MassTransit;
using ShopSphere.Api.Contracts.Events;

namespace ShopSphere.Api.Consumers;

public sealed class InventoryReservationFailedLogger : IConsumer<InventoryReservationFailed>
{
    private readonly ILogger<InventoryReservationFailedLogger> _logger;
    public InventoryReservationFailedLogger(ILogger<InventoryReservationFailedLogger> logger) => _logger = logger;

    public Task Consume(ConsumeContext<InventoryReservationFailed> context)
    {
        var m = context.Message;
        _logger.LogWarning(
            "InventoryReservationFailed | orderId={OrderId} reason={Reason} failures={Count}",
            m.OrderId, m.Reason, m.Failures.Count);
        return Task.CompletedTask;
    }
}