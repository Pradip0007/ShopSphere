using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.Api.Features.Cart;

public static class RemoveItem
{
    public static async Task<IResult> HandleAsync(
        Guid productId,
        HttpContext http,
        ICartRepository carts,
        CancellationToken ct)
    {
        if (productId == Guid.Empty)
        {
            return Results.NotFound();
        }

        var key = CartKeyResolver.From(http);
        await carts.RemoveItemAsync(key, new ProductId(productId), ct);
        var cart = await carts.GetAsync(key, ct);
        return Results.Ok(AddItem.ToResponse(cart));
    }
}