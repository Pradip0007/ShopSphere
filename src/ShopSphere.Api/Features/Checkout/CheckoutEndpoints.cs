using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Checkout;

public sealed class CheckoutEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/checkout")
            .WithTags("Checkout")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/review", CheckoutReview.HandleAsync);
        group.MapPost("/", CheckoutFeature.HandleAsync);
    }
}