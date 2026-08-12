using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.Api.Features.Cart;

public static class UpdateItem
{
    public sealed record Request(int Quantity);

    public static async Task<IResult> HandleAsync(
        Guid productId,
        Request request,
        HttpContext http,
        ICartRepository carts,
        CancellationToken ct)
    {
        if (productId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["productId"] = ["ProductId is required."]
            });
        }

        if (request.Quantity < 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["quantity"] = ["Quantity cannot be negative. Send 0 to remove the line."]
            });
        }

        var key = CartKeyResolver.From(http);
        await carts.UpdateItemAsync(key, new ProductId(productId), request.Quantity, ct);
        var cart = await carts.GetAsync(key, ct);
        return Results.Ok(AddItem.ToResponse(cart));
    }
}