using Grpc.Core;
using ShopSphere.Inventory.Grpc;

namespace ShopSphere.Api.Features.Inventory;

public sealed class GrpcInventoryClient : IInventoryClient
{
    private readonly InventoryService.InventoryServiceClient _client;
    private readonly ILogger<GrpcInventoryClient> _log;

    public GrpcInventoryClient(
        InventoryService.InventoryServiceClient client,
        ILogger<GrpcInventoryClient> log)
    {
        _client = client;
        _log = log;
    }

    public async Task<ReserveResult> ReserveAsync(
        Guid orderId,
        string sku,
        int quantity,
        CancellationToken ct)
    {
        try
        {
            var reply = await _client.ReserveStockAsync(
                new ReserveStockRequest
                {
                    OrderId = orderId.ToString(),
                    Sku = sku,
                    Quantity = quantity
                },
                cancellationToken: ct);

            return new ReserveResult(
                reply.Success,
                reply.Remaining,
                null,
                reply.Message);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _log.LogWarning(
                "Reserve failed: sku {Sku} not found",
                sku);

            return new ReserveResult(
                false,
                0,
                ReserveFailReason.UnknownSku,
                ex.Status.Detail);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            _log.LogWarning(
                "Reserve failed: insufficient stock for {Sku}",
                sku);

            return new ReserveResult(
                false,
                0,
                ReserveFailReason.InsufficientStock,
                ex.Status.Detail);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
        {
            return new ReserveResult(
                false,
                0,
                ReserveFailReason.InvalidRequest,
                ex.Status.Detail);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            _log.LogError(
                ex,
                "Inventory service unavailable");

            return new ReserveResult(
                false,
                0,
                ReserveFailReason.Unavailable,
                ex.Status.Detail);
        }
    }

    public async Task<ReleaseResult> ReleaseAsync(
        Guid orderId,
        string sku,
        int quantity,
        CancellationToken ct)
    {
        var reply = await _client.ReleaseStockAsync(
            new ReleaseStockRequest
            {
                OrderId = orderId.ToString(),
                Sku = sku,
                Quantity = quantity
            },
            cancellationToken: ct);

        return new ReleaseResult(
            reply.Success,
            reply.Remaining);
    }
}