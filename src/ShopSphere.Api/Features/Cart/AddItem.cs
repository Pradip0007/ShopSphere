using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;
using DomainCart = ShopSphere.Domain.Cart.Cart;

namespace ShopSphere.Api.Features.Cart;

public static class AddItem
{
    public sealed record Request(Guid ProductId, int Quantity);

    public static async Task<IResult> HandleAsync(
        Request request,
        HttpContext http,
        ICartRepository carts,
        CancellationToken ct)
    {
        if (request.ProductId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["productId"] = ["ProductId is required."]
            });
        }

        if (request.Quantity < 1)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["quantity"] = ["Quantity must be at least 1."]
            });
        }

        var key = CartKeyResolver.From(http);

        await carts.AddItemAsync(
            key,
            new ProductId(request.ProductId),
            request.Quantity,
            ct);

        var cart = await carts.GetAsync(key, ct);

        return Results.Ok(ToResponse(cart));
    }

    internal static CartResponse ToResponse(DomainCart cart) => new(
        cart.Key.ToRedisKey(),
        cart.TotalUnits,
        cart.Lines
            .Select(l =>
                new CartLineResponse(
                    l.ProductId.Value.ToString("D"),
                    l.Quantity))
            .ToArray());
}