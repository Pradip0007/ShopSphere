using System.Collections.Concurrent;

namespace ShopSphere.Api.Infrastructure.Messaging;

public sealed class InMemoryProcessedMessageStore : IProcessedMessageStore
{
    private readonly ConcurrentDictionary<(Guid, string), byte> _seen = new();

    public Task<bool> TryMarkAsync(
        Guid messageId,
        string consumerName,
        CancellationToken ct = default)
    {
        var added = _seen.TryAdd(
            (messageId, consumerName),
            0);

        return Task.FromResult(added);
    }
}