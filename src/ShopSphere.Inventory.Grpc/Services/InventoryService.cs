using Grpc.Core;

namespace ShopSphere.Inventory.Grpc.Services;

public sealed class InventoryService
    : global::ShopSphere.Inventory.Grpc.InventoryService.InventoryServiceBase
{
    public override Task<ReserveStockResponse> ReserveStock(
        ReserveStockRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new ReserveStockResponse
        {
            Success = false,
            Remaining = 0,
            Message = "Stub implementation — inventory reservation is not connected yet."
        });
    }

    public override Task<ReleaseStockResponse> ReleaseStock(
        ReleaseStockRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new ReleaseStockResponse
        {
            Success = false,
            Remaining = 0,
            Message = "Stub implementation — inventory release is not connected yet."
        });
    }

    public override Task StreamLowStock(
        LowStockRequest request,
        IServerStreamWriter<LowStockEvent> responseStream,
        ServerCallContext context)
    {
        return Task.CompletedTask;
    }
}