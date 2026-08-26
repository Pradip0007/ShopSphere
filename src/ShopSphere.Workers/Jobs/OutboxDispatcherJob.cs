using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Workers.Jobs;

public sealed class OutboxDispatcherJob : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromSeconds(2);
    private const int BatchSize = 50;

    private readonly IServiceScopeFactory _scopes;
    private readonly IBus _bus;
    private readonly ILogger<OutboxDispatcherJob> _logger;
    private readonly TimeProvider _clock;
    private readonly string _claimer = $"{Environment.MachineName}:{Guid.NewGuid():N}";

    public OutboxDispatcherJob(
        IServiceScopeFactory scopes,
        IBus bus,
        ILogger<OutboxDispatcherJob> logger,
        TimeProvider clock)
    {
        _scopes = scopes;
        _bus = bus;
        _logger = logger;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Period);
        do
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatch tick failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DispatchBatchAsync(CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopSphereDbContext>();
        var now = _clock.GetUtcNow();

        await db.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null &&
                        (m.ClaimedAtUtc == null || m.ClaimedAtUtc < now.AddMinutes(-5)) &&
                        m.Attempts < 10)
            .OrderBy(m => m.OccurredAtUtc)
            .Take(BatchSize)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.ClaimedAtUtc, now)
                .SetProperty(m => m.ClaimedBy, _claimer), ct);

        var pending = await db.OutboxMessages
            .Where(m => m.ClaimedBy == _claimer && m.ProcessedAtUtc == null)
            .OrderBy(m => m.OccurredAtUtc)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        foreach (var msg in pending)
        {
            try
            {
                var type = Type.GetType(msg.Type, throwOnError: false);
                if (type is null)
                {
                    throw new InvalidOperationException($"Type '{msg.Type}' not found.");
                }
                var payload = System.Text.Json.JsonSerializer.Deserialize(msg.PayloadJson, type)!;
                await _bus.Publish(payload, ct);
                msg.ProcessedAtUtc = _clock.GetUtcNow();
            }
            catch (Exception ex)
            {
                msg.Attempts++;
                msg.LastError = ex.Message;
                if (msg.Attempts >= 10)
                {
                    msg.ProcessedAtUtc = _clock.GetUtcNow();
                }
                _logger.LogError(ex, "Outbox publish failed for {Id} (attempt {Attempts})", msg.Id, msg.Attempts);
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
