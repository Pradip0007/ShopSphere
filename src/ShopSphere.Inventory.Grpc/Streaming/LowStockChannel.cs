using System.Collections.Concurrent;
using System.Threading.Channels;
using ShopSphere.Inventory.Grpc;

namespace ShopSphere.Inventory.Grpc.Streaming;

public interface ILowStockChannel
{
    ValueTask WriteAsync(
        LowStockEvent evt,
        CancellationToken ct = default);

    IAsyncEnumerable<LowStockEvent> Subscribe(
        int threshold,
        CancellationToken ct);
}

/// <summary>
/// Fan-out broker: every active subscriber receives matching low-stock events.
/// Each subscriber owns a bounded channel so a slow client cannot block others.
/// </summary>
public sealed class LowStockChannel : ILowStockChannel
{
    private sealed record Subscriber(
        int Threshold,
        Channel<LowStockEvent> Channel);

    private readonly ConcurrentDictionary<Guid, Subscriber> _subs = new();

    public async ValueTask WriteAsync(
        LowStockEvent evt,
        CancellationToken ct = default)
    {
        foreach (var sub in _subs.Values)
        {
            if (evt.Available <= sub.Threshold)
            {
                await sub.Channel.Writer.WriteAsync(evt, ct);
            }
        }
    }

    public async IAsyncEnumerable<LowStockEvent> Subscribe(
        int threshold,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken ct)
    {
        var id = Guid.NewGuid();

        var channel = Channel.CreateBounded<LowStockEvent>(
            new BoundedChannelOptions(256)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });

        var sub = new Subscriber(threshold, channel);

        _subs[id] = sub;

        try
        {
            await foreach (var evt in channel.Reader.ReadAllAsync(ct))
            {
                yield return evt;
            }
        }
        finally
        {
            _subs.TryRemove(id, out _);
            channel.Writer.TryComplete();
        }
    }
}