using MassTransit;
using ShopSphere.Api.Contracts.Events;

namespace ShopSphere.Api.Consumers;

public sealed class InventoryReservedLogger : IConsumer<InventoryReserved>
{
    private readonly ILogger<InventoryReservedLogger> _logger;
    public InventoryReservedLogger(ILogger<InventoryReservedLogger> logger) => _logger = logger;

    public Task Consume(ConsumeContext<InventoryReserved> context)
    {
        _logger.LogInformation("InventoryReserved observed | orderId={OrderId}", context.Message.OrderId);
        return Task.CompletedTask;
    }
}