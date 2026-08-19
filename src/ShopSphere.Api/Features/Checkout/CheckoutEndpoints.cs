namespace ShopSphere.Api.Features.Checkout;

public static class CheckoutEndpoints
{
    public static IEndpointRouteBuilder MapCheckoutEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/checkout")
            .WithTags("Checkout")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", CheckoutFeature.HandleAsync);

        return routes;
    }
}