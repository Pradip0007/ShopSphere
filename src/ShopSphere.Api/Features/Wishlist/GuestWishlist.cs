using StackExchange.Redis;

namespace ShopSphere.Api.Features.Wishlist;

public sealed class GuestWishlistStore(IConnectionMultiplexer redis)
{
    private const string KeyPrefix = "wishlist:s:";

    public Task AddAsync(Guid sessionId, Guid productId)
    {
        return redis
            .GetDatabase()
            .SetAddAsync(
                KeyPrefix + sessionId,
                productId.ToString("D"));
    }

    public async Task<IReadOnlyList<Guid>> DrainAsync(Guid sessionId)
    {
        var db = redis.GetDatabase();
        var key = KeyPrefix + sessionId;

        RedisValue[] members = await db.SetMembersAsync(key);

        await db.KeyDeleteAsync(key);

        return members
            .Where(value => Guid.TryParse(value.ToString(), out _))
            .Select(value => Guid.Parse(value.ToString()))
            .ToArray();
    }

    public Task RemoveAsync(Guid sessionId, Guid productId)
    {
        return redis
            .GetDatabase()
            .SetRemoveAsync(
                KeyPrefix + sessionId,
                productId.ToString("D"));
    }
}