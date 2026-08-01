using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.ListProducts;

public sealed class ListProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", () => Results.Ok(Array.Empty<object>()))
           .WithName("ListProducts")
           .WithFeature("Catalog");
    }
}