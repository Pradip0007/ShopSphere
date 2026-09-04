using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Reviews;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Catalog.ListProductReviews;

/// <summary>
/// Product-page review projection addressed by product slug, matching the
/// catalogue navigation URL rather than requiring the client to know an ID.
/// </summary>
public sealed class ListProductReviewsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{slug}/reviews", async (
                string slug,
                int? page,
                int? pageSize,
                ShopSphereDbContext db,
                CancellationToken ct) =>
            {
                int requestedPage = Math.Max(page ?? 1, 1);
                int requestedPageSize = Math.Clamp(pageSize ?? 10, 1, 100);

                Guid? productId = await db.Products
                    .AsNoTracking()
                    .Where(product => product.Slug.Value == slug)
                    .Select(product => (Guid?)product.Id.Value)
                    .SingleOrDefaultAsync(ct);

                if (productId is null)
                {
                    return Results.NotFound();
                }

                IQueryable<ProductReviewListItem> reviews =
                    from review in db.Reviews.AsNoTracking()
                    join user in db.Users.AsNoTracking() on review.UserId equals user.Id.Value
                    where review.ProductId.Value == productId.Value && review.Status == ReviewStatus.Approved
                    orderby review.PostedAtUtc descending
                    select new ProductReviewListItem(
                        review.Id.Value,
                        user.Email,
                        review.Rating,
                        string.Empty,
                        review.Body,
                        review.PostedAtUtc);

                int totalCount = await reviews.CountAsync(ct);
                List<ProductReviewListItem> items = await reviews
                    .Skip((requestedPage - 1) * requestedPageSize)
                    .Take(requestedPageSize)
                    .ToListAsync(ct);

                return Results.Ok(new PagedResult<ProductReviewListItem>(
                    items,
                    requestedPage,
                    requestedPageSize,
                    totalCount));
            })
            .WithName("ListProductReviewsBySlug")
            .WithFeature("Catalog")
            .WithSummary("List approved product reviews")
            .Produces<PagedResult<ProductReviewListItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed record ProductReviewListItem(
    Guid Id,
    string AuthorDisplayName,
    int Rating,
    string Title,
    string Body,
    DateTimeOffset CreatedUtc);
