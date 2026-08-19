using MassTransit;

namespace ShopSphere.Api.Consumers;

public sealed class PingConsumer : IConsumer<PingCommand>
{
    private readonly ILogger<PingConsumer> _logger;

    public PingConsumer(ILogger<PingConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PingCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "PingConsumer received {Id} note={Note} emittedAt={EmittedAt} messageId={MessageId}",
            msg.Id, msg.Note, msg.EmittedAtUtc, context.MessageId);
        return Task.CompletedTask;
    }
}
