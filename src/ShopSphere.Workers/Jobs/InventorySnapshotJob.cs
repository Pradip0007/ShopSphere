using ShopSphere.Api.Infrastructure.Inventory;
using StackExchange.Redis;

namespace ShopSphere.Workers.Jobs;

public sealed class InventorySnapshotJob : BackgroundService
{
    private const string SnapshotKey = "inventory:snapshot";
    private static readonly TimeSpan Period = TimeSpan.FromMinutes(5);

    private readonly IConnectionMultiplexer _mux;
    private readonly IStockService _stock;
    private readonly ILogger<InventorySnapshotJob> _logger;

    public InventorySnapshotJob(
        IConnectionMultiplexer mux,
        IStockService stock,
        ILogger<InventorySnapshotJob> logger)
    {
        _mux = mux;
        _stock = stock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(Period);
        do
        {
            try
            {
                var written = await SnapshotOnceAsync(stoppingToken);
                _logger.LogInformation("Inventory snapshot updated | rows={Rows}", written);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inventory snapshot failed. Will retry next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task<int> SnapshotOnceAsync(CancellationToken ct)
    {
        var db = _mux.GetDatabase();
        // Write to a scratch key then rename — readers never see a partial snapshot.
        var scratch = $"{SnapshotKey}:tmp:{Guid.NewGuid():N}";

        var rows = 0;
        var buffer = new List<HashEntry>(capacity: 128);

        await foreach (var s in _stock.ListAllAsync(ct))
        {
            buffer.Add(new HashEntry(s.Sku, s.Available));
            rows++;

            if (buffer.Count == 500)
            {
                await db.HashSetAsync(scratch, buffer.ToArray());
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
        {
            await db.HashSetAsync(scratch, buffer.ToArray());
        }

        if (rows == 0)
        {
            // Nothing to publish — do NOT clobber the existing snapshot.
            await db.KeyDeleteAsync(scratch);
            return 0;
        }

        // TODO(Day 88): replace polling with pub/sub — Inventory publishes stock deltas
        // and this job becomes an event handler that patches the snapshot incrementally.
        await db.KeyRenameAsync(scratch, SnapshotKey);
        return rows;
    }
}
