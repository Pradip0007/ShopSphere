using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Cart;

public sealed class CartEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cart")
            .WithTags("Cart")
            .WithOpenApi()
            .AllowAnonymous();

        group.MapGet("/", GetCart.HandleAsync);
        group.MapPost("/items", AddItem.HandleAsync);
        group.MapPatch("/items/{productId:guid}", UpdateItem.HandleAsync);
        group.MapDelete("/items/{productId:guid}", RemoveItem.HandleAsync);
    }
}