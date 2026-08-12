using ShopSphere.Domain.Cart;

namespace ShopSphere.Api.Features.Cart;

public static class GetCart
{
    public static async Task<IResult> HandleAsync(
        HttpContext http,
        ICartRepository carts,
        CancellationToken ct)
    {
        var key = CartKeyResolver.From(http);
        var cart = await carts.GetAsync(key, ct);
        return Results.Ok(AddItem.ToResponse(cart));
    }
}