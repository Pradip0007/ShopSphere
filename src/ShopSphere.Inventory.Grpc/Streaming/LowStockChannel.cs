using System.Threading.Channels;
using ShopSphere.Inventory.Grpc;

namespace ShopSphere.Inventory.Grpc.Streaming;

public interface ILowStockChannel
{
    ValueTask WriteAsync(
        LowStockEvent evt,
        CancellationToken ct = default);

    ChannelReader<LowStockEvent> Reader { get; }
}

public sealed class LowStockChannel : ILowStockChannel
{
    private readonly Channel<LowStockEvent> _channel =
        Channel.CreateUnbounded<LowStockEvent>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

    public ValueTask WriteAsync(
        LowStockEvent evt,
        CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(evt, ct);

    public ChannelReader<LowStockEvent> Reader =>
        _channel.Reader;
}