using System.Collections.Concurrent;

namespace ShopSphere.Api.Infrastructure.Payments;

public sealed class InMemoryProcessedWebhookStore : IProcessedWebhookStore
{
    private readonly ConcurrentDictionary<string, byte> _seen = new(StringComparer.Ordinal);

    public Task<bool> TryMarkAsync(string stripeEventId, CancellationToken ct = default)
    {
        return Task.FromResult(_seen.TryAdd(stripeEventId, 0));
    }
}