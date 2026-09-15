using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Inventory;
using ShopSphere.Infrastructure.Persistence;
using MassTransit;
using ShopSphere.Contracts.Events;
using ShopSphere.Inventory.Grpc.Streaming;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.Inventory.Grpc.Services;

public sealed class InventoryService(
    ShopSphereDbContext db,
    IPublishEndpoint publishEndpoint,
    ILowStockChannel lowStockChannel,
    TimeProvider clock,
    ILogger<InventoryService> logger)
    : global::ShopSphere.Inventory.Grpc.InventoryService.InventoryServiceBase
{
    private const int LowStockThreshold = 5;
    public override async Task<ReserveStockResponse> ReserveStock(
        ReserveStockRequest request,
        ServerCallContext context)
    {
        ValidateReserveRequest(request);

        var orderId = Guid.Parse(request.OrderId);

        var sku = Sku.From(request.Sku);

        var stock = await db.StockLevels
            .SingleOrDefaultAsync(
                stockLevel => stockLevel.Sku == sku,
                context.CancellationToken);

        if (stock is null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Stock level not found for SKU '{request.Sku}'."));
        }

        var existingReservation = await db.StockReservations
            .SingleOrDefaultAsync(
                reservation =>
                    reservation.OrderId == orderId &&
                    reservation.Sku == request.Sku,
                context.CancellationToken);

        if (existingReservation is not null)
        {
            return new ReserveStockResponse
            {
                Success = true,
                Remaining = stock.Available,
                Message = "Stock already reserved for this order and SKU."
            };
        }

        var result = stock.Reserve(request.Quantity);

        if (result.IsFailure)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                result.Error.Message));
        }

        db.StockReservations.Add(
            new StockReservation(
                orderId,
                request.Sku,
                request.Quantity,
                clock.GetUtcNow()));

        await db.SaveChangesAsync(context.CancellationToken);

        await publishEndpoint.Publish(
        new StockReserved(
            orderId,
            request.Sku,
            request.Quantity,
            stock.Available,
            clock.GetUtcNow()),
        context.CancellationToken);

        await MaybeEmitLowStockAsync(
        stock,
        context);

        logger.LogInformation(
            "Reserved {Quantity} units of {Sku} for order {OrderId}. Remaining: {Remaining}",
            request.Quantity,
            request.Sku,
            orderId,
            stock.Available);

        return new ReserveStockResponse
        {
            Success = true,
            Remaining = stock.Available,
            Message = "Stock reserved successfully."
        };
    }

    private static void ValidateReserveRequest(ReserveStockRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "SKU is required."));
        }

        if (request.Quantity <= 0)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Quantity must be a positive integer."));
        }

        if (!Guid.TryParse(request.OrderId, out _))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Order ID must be a valid GUID."));
        }
    }

    public override async Task<ReleaseStockResponse> ReleaseStock(
    ReleaseStockRequest request,
    ServerCallContext context)
    {
        ValidateReleaseRequest(request);

        var orderId = Guid.Parse(request.OrderId);

        var sku = Sku.From(request.Sku);

        var stock = await db.StockLevels
            .SingleOrDefaultAsync(
                stockLevel => stockLevel.Sku == sku,
                context.CancellationToken);

        if (stock is null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Stock level not found for SKU '{request.Sku}'."));
        }

        var reservation = await db.StockReservations
            .SingleOrDefaultAsync(
                existing =>
                    existing.OrderId == orderId &&
                    existing.Sku == request.Sku,
                context.CancellationToken);

        if (reservation is null)
        {
            return new ReleaseStockResponse
            {
                Success = true,
                Remaining = stock.Available,
                Message = "Stock reservation was already released."
            };
        }

        var result = stock.Release(reservation.Quantity);

        if (result.IsFailure)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                result.Error.Message));
        }

        db.StockReservations.Remove(reservation);

        await db.SaveChangesAsync(context.CancellationToken);

        await publishEndpoint.Publish(
        new StockReleased(
            orderId,
            request.Sku,
            reservation.Quantity,
            stock.Available,
            clock.GetUtcNow()),
        context.CancellationToken);

        logger.LogInformation(
            "Released {Quantity} units of {Sku} for order {OrderId}. Remaining: {Remaining}",
            reservation.Quantity,
            request.Sku,
            orderId,
            stock.Available);

        return new ReleaseStockResponse
        {
            Success = true,
            Remaining = stock.Available,
            Message = "Stock released successfully."
        };
    }
    private static void ValidateReleaseRequest(ReleaseStockRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "SKU is required."));
        }

        if (request.Quantity <= 0)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Quantity must be a positive integer."));
        }

        if (!Guid.TryParse(request.OrderId, out _))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Order ID must be a valid GUID."));
        }
    }

    public override async Task StreamLowStock(
    LowStockRequest request,
    IServerStreamWriter<LowStockEvent> responseStream,
    ServerCallContext context)
    {
        var threshold = request.Threshold > 0
            ? request.Threshold
            : LowStockThreshold;

        await foreach (var evt in lowStockChannel.Reader.ReadAllAsync(
            context.CancellationToken))
        {
            if (evt.Available <= threshold)
            {
                await responseStream.WriteAsync(evt);
            }
        }
    }

    private async Task MaybeEmitLowStockAsync(
    StockLevel stock,
    ServerCallContext context)
    {
        if (stock.Available > LowStockThreshold)
        {
            return;
        }

        var evt = new LowStockEvent
        {
            Sku = stock.Sku.Value,
            Available = stock.Available,
            Timestamp = clock.GetUtcNow().ToString("O")
        };

        await lowStockChannel.WriteAsync(
            evt,
            context.CancellationToken);
    }
}