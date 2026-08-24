namespace ShopSphere.Api.Infrastructure.Inventory;

public interface IStockService
{
    Task<ReserveResult> ReserveAsync(Guid productId, int quantity, CancellationToken ct = default);

    IAsyncEnumerable<StockSnapshot> ListAllAsync(CancellationToken ct = default);
}

public sealed record ReserveResult(bool Succeeded, string Sku, int Available);

public sealed record StockSnapshot(Guid ProductId, string Sku, int Available);