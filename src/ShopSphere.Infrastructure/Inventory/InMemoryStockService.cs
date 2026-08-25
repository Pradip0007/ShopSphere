using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ShopSphere.Infrastructure.Inventory;

public sealed class InMemoryStockService : IStockService
{
    private readonly ConcurrentDictionary<Guid, StockRow> _rows = new();

    public Task<ReserveResult> ReserveAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        var row = _rows.GetOrAdd(productId, id => new StockRow($"SKU-{id:N}"[..12], 100));
        lock (row)
        {
            if (row.Available < quantity)
            {
                return Task.FromResult(new ReserveResult(false, row.Sku, row.Available));
            }
            row.Available -= quantity;
            return Task.FromResult(new ReserveResult(true, row.Sku, row.Available));
        }
    }

    public async IAsyncEnumerable<StockSnapshot> ListAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var kvp in _rows)
        {
            ct.ThrowIfCancellationRequested();
            var row = kvp.Value;
            yield return new StockSnapshot(kvp.Key, row.Sku, row.Available);
            await Task.Yield();
        }
    }

    private sealed class StockRow(string sku, int available)
    {
        public string Sku { get; } = sku;
        public int Available { get; set; } = available;
    }
}