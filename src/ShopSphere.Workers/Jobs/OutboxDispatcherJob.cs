using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Infrastructure.Outbox;

namespace ShopSphere.Workers.Jobs;

public sealed class OutboxDispatcherJob : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromSeconds(2);
    private const int BatchSize = 50;

    private readonly IServiceScopeFactory _scopes;
    private readonly IBus _bus;
    private readonly ILogger<OutboxDispatcherJob> _logger;

    public OutboxDispatcherJob(
        IServiceScopeFactory scopes,
        IBus bus,
        ILogger<OutboxDispatcherJob> logger)
    {
        _scopes = scopes;
        _bus = bus;
        _logger = logger;
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
        var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

        var pending = await db.Outbox
            .Where(m => m.ProcessedAtUtc == null)
            .ToListAsync(ct);

        pending = pending
            .OrderBy(m => m.OccurredAtUtc)
            .Take(BatchSize)
            .ToList();

        if (pending.Count == 0) return;

        foreach (var msg in pending)
        {
            try
            {
                var type = Type.GetType(msg.Type, throwOnError: true)!;
                var payload = System.Text.Json.JsonSerializer.Deserialize(msg.PayloadJson, type)!;
                await _bus.Publish(payload, ct);
                msg.ProcessedAtUtc = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                msg.Attempts++;
                msg.LastError = ex.Message;
                _logger.LogError(ex, "Outbox publish failed for {Id} (attempt {Attempts})", msg.Id, msg.Attempts);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
