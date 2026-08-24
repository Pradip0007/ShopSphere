using MassTransit;
using ShopSphere.Api.Contracts.Events;
using StackExchange.Redis;

namespace ShopSphere.Workers.Jobs;

public sealed class AbandonedCartReminderJob : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromHours(1);
    private static readonly TimeSpan IdleThreshold = TimeSpan.FromHours(24);
    private readonly IConnectionMultiplexer _mux;
    private readonly IBus _bus;
    private readonly ILogger<AbandonedCartReminderJob> _logger;

    public AbandonedCartReminderJob(
        IConnectionMultiplexer mux,
        IBus bus,
        ILogger<AbandonedCartReminderJob> logger)
    {
        _mux = mux;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(
                TimeSpan.FromSeconds(15),
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(Period);

        do
        {
            try
            {
                await ScanOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Abandoned cart scan failed. Will retry next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ScanOnceAsync(CancellationToken ct)
    {
        var db = _mux.GetDatabase();
        var reminded = 0;

        foreach (var endpoint in _mux.GetEndPoints())
        {
            var server = _mux.GetServer(endpoint);

            if (!server.IsConnected || server.IsReplica)
                continue;

            await foreach (
                var key in server
                    .KeysAsync(pattern: "cart:*", pageSize: 200)
                    .WithCancellation(ct))
            {
                ct.ThrowIfCancellationRequested();

                var idle = await db.KeyIdleTimeAsync(key);

                if (idle is null || idle < IdleThreshold)
                    continue;

                var lineCount =
                    (int)await db.HashLengthAsync(key);

                if (lineCount == 0)
                    continue;

                var reminder =
                    ToEvent(key!, idle.Value, lineCount);

                await _bus.Publish(reminder, ct);

                reminded++;
            }
        }

        if (reminded > 0)
        {
            _logger.LogInformation(
                "Published {Count} abandoned-cart reminders.",
                reminded);
        }
    }

    private static AbandonedCartReminder ToEvent(
        string redisKey,
        TimeSpan idle,
        int lineCount)
    {
        Guid? userId = null;

        var parts = redisKey.Split(':', 3);

        if (parts.Length == 3 &&
            parts[1] == "u" &&
            Guid.TryParse(parts[2], out var uid))
        {
            userId = uid;
        }

        return new AbandonedCartReminder(
            CartKey: redisKey,
            UserId: userId,
            LineCount: lineCount,
            IdleFor: idle,
            DetectedAtUtc: DateTimeOffset.UtcNow);
    }
}