using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;
using StackExchange.Redis;
using DomainCart = ShopSphere.Domain.Cart.Cart;

namespace ShopSphere.Api.Infrastructure.Cart;

public sealed class RedisCartRepository : ICartRepository
{
    private static readonly TimeSpan CartTtl = TimeSpan.FromDays(30);

    private readonly IDatabase _db;

    public RedisCartRepository(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }

    public async Task<DomainCart> GetAsync(CartKey key, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var entries = await _db.HashGetAllAsync(key.ToRedisKey()).ConfigureAwait(false);
        var lines = new List<CartLine>(entries.Length);

        foreach (var entry in entries)
        {
            if (!Guid.TryParse(entry.Name.ToString(), out var productGuid)) continue;
            if (!int.TryParse(entry.Value.ToString(), out var qty)) continue;
            if (qty <= 0) continue;
            lines.Add(new CartLine(new ProductId(productGuid), qty));
        }

        return new DomainCart(key, lines);
    }

    public async Task AddItemAsync(
        CartKey key,
        ProductId productId,
        int quantity,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        ct.ThrowIfCancellationRequested();

        var redisKey = key.ToRedisKey();
        var newQty = await _db.HashIncrementAsync(
            redisKey,
            productId.Value.ToString("D"),
            quantity).ConfigureAwait(false);

        // Refresh TTL on every write. A cart lives 30 days from the last touch.
        await _db.KeyExpireAsync(redisKey, CartTtl).ConfigureAwait(false);

        if (newQty <= 0)
        {
            // Defensive: negative or zero after increment means an inconsistent state — drop the field.
            await _db.HashDeleteAsync(redisKey, productId.Value.ToString("D")).ConfigureAwait(false);
        }
    }

    public async Task UpdateItemAsync(
        CartKey key,
        ProductId productId,
        int quantity,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        ct.ThrowIfCancellationRequested();

        var redisKey = key.ToRedisKey();
        var field = productId.Value.ToString("D");

        if (quantity == 0)
        {
            await _db.HashDeleteAsync(redisKey, field).ConfigureAwait(false);
        }
        else
        {
            await _db.HashSetAsync(redisKey, field, quantity).ConfigureAwait(false);
            await _db.KeyExpireAsync(redisKey, CartTtl).ConfigureAwait(false);
        }
    }

    public Task RemoveItemAsync(CartKey key, ProductId productId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _db.HashDeleteAsync(key.ToRedisKey(), productId.Value.ToString("D"));
    }

    public Task ClearAsync(CartKey key, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _db.KeyDeleteAsync(key.ToRedisKey());
    }
}