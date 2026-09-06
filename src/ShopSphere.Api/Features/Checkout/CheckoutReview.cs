using System.Security.Claims;
using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.Api.Features.Checkout;

public static class CheckoutReview
{
    public sealed record Item(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        string Currency,
        decimal LineTotal);

    public sealed record Response(
        IReadOnlyList<Item> Items,
        decimal Subtotal,
        string Currency);

    public static async Task<IResult> HandleAsync(
        HttpContext http,
        ICartRepository carts,
        IProductRepository products,
        CancellationToken ct)
    {
        var userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? http.User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var cart = await carts.GetAsync(CartKey.User(userId), ct);

        if (cart.IsEmpty)
        {
            return Results.BadRequest(new { error = "Cart is empty." });
        }

        var items = new List<Item>(cart.Lines.Count);

        foreach (var line in cart.Lines)
        {
            var product = await products.FindAsync(line.ProductId, ct);

            if (product is null)
            {
                return Results.BadRequest(
                    new { error = $"Product {line.ProductId} no longer exists." });
            }

            var lineTotal = product.Price * line.Quantity;

            items.Add(
                new Item(
                    product.Id.Value,
                    product.Title,
                    line.Quantity,
                    product.Price.Amount,
                    product.Price.Currency,
                    lineTotal.Amount));
        }

        var currency = items[0].Currency;
        var subtotal = items.Sum(item => item.LineTotal);

        return Results.Ok(
            new Response(
                items,
                subtotal,
                currency));
    }
}