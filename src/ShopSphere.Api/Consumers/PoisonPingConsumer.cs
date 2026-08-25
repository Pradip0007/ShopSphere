using MassTransit;

namespace ShopSphere.Api.Consumers;

// TEMP — Day 47 DLQ demonstration.
public sealed record PoisonPing(Guid Id);

public sealed class PoisonPingConsumer : IConsumer<PoisonPing>
{
    public Task Consume(ConsumeContext<PoisonPing> context)
    {
        throw new InvalidOperationException(
            "Intentional failure for DLQ demo.");
    }
}