using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.ListProducts;

public sealed class ListProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
                int? page,
                int? pageSize,
                Guid? categoryId,
                decimal? minPrice,
                decimal? maxPrice,
                string? sort,
                ISender sender,
                CancellationToken ct) =>
            {
                ListProductsQuery query = new(
                    page ?? 1,
                    pageSize ?? 20,
                    categoryId,
                    minPrice,
                    maxPrice,
                    sort);

                PagedResult<ProductListItem> result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("ListProducts")
            .WithFeature("Catalog")
            .WithSummary("List products")
            .WithDescription("Returns a paged list of products with optional filtering by category, price range, and sort order.")
            .Produces<PagedResult<ProductListItem>>(StatusCodes.Status200OK);
    }
}