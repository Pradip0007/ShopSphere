using Grpc.Net.Client;
using ShopSphere.Inventory.Grpc;

var address = args.FirstOrDefault() ?? "https://localhost:7100";

var threshold =
    args.Length > 1 && int.TryParse(args[1], out var parsedThreshold)
        ? parsedThreshold
        : 5;

Console.WriteLine(
    $"Watching {address} for stock <= {threshold}. Ctrl+C to stop.");

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

using var channel = GrpcChannel.ForAddress(address);

var client = new InventoryService.InventoryServiceClient(channel);

using var call = client.StreamLowStock(
    new LowStockRequest
    {
        Threshold = threshold
    },
    cancellationToken: cts.Token);

try
{
    while (await call.ResponseStream.MoveNext(cts.Token))
    {
        var evt = call.ResponseStream.Current;

        Console.WriteLine(
            $"{evt.Timestamp}  {evt.Sku,-16} available={evt.Available,3}  (t={threshold})");
    }
}
catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
{
    Console.WriteLine("Stopped.");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stopped.");
}