using System.Security.Claims;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Features.Orders.ListOrders;

public sealed class ListOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (
                int? page,
                int? pageSize,
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

                var currentPage = page ?? 1;
                var currentPageSize = pageSize ?? 20;

                if (currentPage < 1)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["page"] = ["Page must be greater than 0."]
                    });
                }

                if (currentPageSize is < 1 or > 100)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["pageSize"] = ["PageSize must be between 1 and 100."]
                    });
                }

                var (ordersPage, totalCount) = await orders.GetByUserAsync(
                    userId,
                    currentPage,
                    currentPageSize,
                    ct);

                var items = ordersPage
                    .Select(MapSummary)
                    .ToArray();

                return Results.Ok(
                    new PagedResult<OrderSummary>(
                        items,
                        currentPage,
                        currentPageSize,
                        totalCount));
            })
            .WithName("ListOrders")
            .WithFeature("Orders")
            .WithSummary("List current user's orders")
            .WithDescription("Returns a paged list of orders belonging to the authenticated user.")
            .RequireAuthorization()
            .Produces<PagedResult<OrderSummary>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();
    }

    private static OrderSummary MapSummary(Order order) =>
        new(
            order.Id.Value,
            order.Id.Value.ToString("D"),
            MapStatus(order.Status),
            order.PlacedAtUtc,
            order.Subtotal.Amount,
            order.Currency,
            order.Items.Sum(item => item.Quantity));

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

    public sealed record OrderSummary(
        Guid Id,
        string Number,
        string Status,
        DateTimeOffset PlacedUtc,
        decimal TotalAmount,
        string Currency,
        int ItemCount);
}