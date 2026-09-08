using System.Security.Claims;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Features.Orders.GetOrder;

public sealed class GetOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders/{id:guid}",
                async (
                    Guid id,
                    HttpContext http,
                    IOrderRepository orders,
                    CancellationToken ct) =>
                {
                    var userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? http.User.FindFirst("sub")?.Value;

                    if (!Guid.TryParse(userIdClaim, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var order = await orders.FindAsync(new OrderId(id), ct);

                    if (order is null || order.UserId != userId)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(MapDetail(order));
                })
            .WithName("GetOrder")
            .WithFeature("Orders")
            .WithSummary("Get current user's order")
            .WithDescription("Returns a single order belonging to the authenticated user.")
            .RequireAuthorization()
            .Produces<OrderDetail>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static OrderDetail MapDetail(Order order)
    {
        var shippingAddress = new OrderAddress(
            string.Empty,
            order.ShippingAddress.Line1,
            order.ShippingAddress.Line2,
            order.ShippingAddress.City,
            order.ShippingAddress.PostalCode,
            order.ShippingAddress.Country,
            null);

        var lines = order.Items
            .Select(
                line =>
                    new OrderLine(
                        line.ProductId.Value,
                        string.Empty,
                        line.ProductNameSnapshot,
                        null,
                        line.Quantity,
                        line.UnitPriceSnapshot.Amount,
                        line.LineTotal.Amount,
                        line.UnitPriceSnapshot.Currency))
            .ToArray();

        return new OrderDetail(
            order.Id.Value,
            order.Id.Value.ToString("D"),
            MapStatus(order.Status),
            order.PlacedAtUtc,
            order.PlacedAtUtc,
            lines,
            order.Subtotal.Amount,
            null,
            null,
            order.Subtotal.Amount,
            order.Currency,
            shippingAddress,
            null,
            null);
    }

    private static string MapStatus(OrderStatus status) =>
        status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.InventoryReserved => "Pending",
            OrderStatus.PaymentAuthorized => "Paid",
            OrderStatus.Confirmed => "Confirmed",
            OrderStatus.Shipped => "Shipped",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.RefundRequested => "Refunded",
            _ => "Pending",
        };

    public sealed record OrderLine(
        Guid ProductId,
        string ProductSlug,
        string ProductName,
        string? ImageUrl,
        int Quantity,
        decimal UnitPriceAmount,
        decimal LineTotalAmount,
        string Currency);

    public sealed record OrderAddress(
        string FullName,
        string Line1,
        string? Line2,
        string City,
        string PostalCode,
        string Country,
        string? Phone);

    public sealed record OrderDetail(
        Guid Id,
        string Number,
        string Status,
        DateTimeOffset PlacedUtc,
        DateTimeOffset UpdatedUtc,
        IReadOnlyList<OrderLine> Lines,
        decimal SubtotalAmount,
        decimal? ShippingAmount,
        decimal? TaxAmount,
        decimal TotalAmount,
        string Currency,
        OrderAddress ShippingAddress,
        OrderAddress? BillingAddress,
        string? TrackingNumber);
}