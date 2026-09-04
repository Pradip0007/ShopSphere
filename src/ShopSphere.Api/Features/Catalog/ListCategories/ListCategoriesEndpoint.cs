using Microsoft.EntityFrameworkCore;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.ListCategories;

public sealed class ListCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/categories", async (
                ShopSphereDbContext db,
                CancellationToken ct) =>
            {
                var categories = await db.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryListItem(
                        c.Id.Value,
                        c.Name,
                        c.Slug.Value))
                    .ToListAsync(ct);

                return Results.Ok(categories);
            })
            .WithName("ListProductCategories")
            .WithFeature("Catalog")
            .WithSummary("List product categories")
            .WithDescription("Returns product categories for catalog filtering.")
            .Produces<IReadOnlyList<CategoryListItem>>(StatusCodes.Status200OK);
    }
}

public sealed record CategoryListItem(
    Guid Id,
    string Name,
    string Slug);