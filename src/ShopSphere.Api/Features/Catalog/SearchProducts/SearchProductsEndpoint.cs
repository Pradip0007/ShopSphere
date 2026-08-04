using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Features.Catalog.ListProducts;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.SearchProducts;

public sealed class SearchProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/search", async (
                string q,
                int? page,
                int? pageSize,
                ISender sender,
                CancellationToken ct) =>
            {
                SearchProductsQuery query = new(q, page ?? 1, pageSize ?? 20);
                PagedResult<ProductListItem> result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("SearchProducts")
            .WithFeature("Catalog")
            .WithSummary("Search products")
            .WithDescription("Full-text-ish search over product Title and Description. LIKE-based — will be replaced with embeddings on Day 85.")
            .Produces<PagedResult<ProductListItem>>(StatusCodes.Status200OK);
    }
}