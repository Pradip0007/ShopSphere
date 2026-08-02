using MediatR;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.ListProducts;

public sealed class ListProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
                int? page,
                int? pageSize,
                ISender sender,
                CancellationToken ct) =>
            {
                ListProductsQuery query = new(page ?? 1, pageSize ?? 20);
                PagedResult<ProductListItem> result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("ListProducts")
            .WithFeature("Catalog")
            .WithSummary("List products")
            .WithDescription("Returns a paged list of active products, ordered by title.")
            .Produces<PagedResult<ProductListItem>>(StatusCodes.Status200OK);
    }
}