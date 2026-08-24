using MassTransit;
using ShopSphere.Api.Contracts.Events;

namespace ShopSphere.Api.Consumers;

public sealed class AbandonedCartReminderLogger : IConsumer<AbandonedCartReminder>
{
    private readonly ILogger<AbandonedCartReminderLogger> _logger;
    public AbandonedCartReminderLogger(ILogger<AbandonedCartReminderLogger> logger) => _logger = logger;

    public Task Consume(ConsumeContext<AbandonedCartReminder> context)
    {
        var m = context.Message;
        _logger.LogInformation(
            "AbandonedCartReminder | cart={CartKey} userId={UserId} lines={Lines} idle={Idle}",
            m.CartKey, m.UserId, m.LineCount, m.IdleFor);
        return Task.CompletedTask;
    }
}