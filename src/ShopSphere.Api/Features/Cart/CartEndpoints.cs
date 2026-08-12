namespace ShopSphere.Api.Features.Cart;

public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/cart")
            .WithTags("Cart")
            .WithOpenApi();

        group.MapGet("/", GetCart.HandleAsync);
        group.MapPost("/items", AddItem.HandleAsync);
        group.MapPatch("/items/{productId:guid}", UpdateItem.HandleAsync);
        group.MapDelete("/items/{productId:guid}", RemoveItem.HandleAsync);

        return routes;
    }
}