using System.Security.Claims;
using ShopSphere.Api.Features.Cart;
using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Features.Checkout;

public static class CheckoutFeature
{
    public sealed record AddressDto(string Line1, string? Line2, string City, string PostalCode, string Country);

    public sealed record Request(AddressDto ShippingAddress);

    public sealed record Response(Guid OrderId, decimal Total, string Currency, int LineCount);

    public static async Task<IResult> HandleAsync(
        Request request,
        HttpContext http,
        ICartRepository carts,
        IProductRepository products,
        IOrderRepository orders,
        CancellationToken ct)
    {
        var userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? http.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var cartKey = CartKey.User(userId);
        var cart = await carts.GetAsync(cartKey, ct);
        if (cart.IsEmpty)
        {
            return Results.BadRequest(new { error = "Cart is empty." });
        }

        var lines = new List<OrderItem>(cart.Lines.Count);
        foreach (var line in cart.Lines)
        {
            var product = await products.FindAsync(line.ProductId, ct);
            if (product is null)
            {
                return Results.BadRequest(new { error = $"Product {line.ProductId} no longer exists." });
            }

            lines.Add(new OrderItem(
                        product.Id,
                        product.Sku.Value,
                        product.Title,
                        product.Price,
                        line.Quantity));
        }

        var address = new Address(
            request.ShippingAddress.Line1,
            request.ShippingAddress.Line2,
            request.ShippingAddress.City,
            request.ShippingAddress.PostalCode,
            request.ShippingAddress.Country);

        var order = Order.Place(userId, lines, address);

        await orders.AddAsync(order, ct);
        await orders.SaveChangesAsync(ct);
        await carts.ClearAsync(cartKey, ct);

        return Results.Created(
            $"/api/v1/orders/{order.Id.Value:D}",
            new Response(order.Id.Value, order.Subtotal.Amount, order.Subtotal.Currency, order.Items.Count));
    }
}