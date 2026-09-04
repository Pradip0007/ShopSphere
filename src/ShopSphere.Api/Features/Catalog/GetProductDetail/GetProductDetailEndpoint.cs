using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Reviews;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Catalog.GetProductDetail;

/// <summary>
/// Public product page projection. It deliberately composes catalog, stock, and
/// approved-review read models without exposing the domain aggregates directly.
/// </summary>
public sealed class GetProductDetailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{slug}", async (
                string slug,
                ShopSphereDbContext db,
                CancellationToken ct) =>
            {
                ProductDetailProjection? product = await (
                    from p in db.Products.AsNoTracking()
                    join category in db.Categories.AsNoTracking() on p.CategoryId equals category.Id
                    join stock in db.StockLevels.AsNoTracking() on p.Id equals stock.ProductId into stocks
                    from stock in stocks.DefaultIfEmpty()
                    where p.Status == ProductStatus.Published && p.Slug.Value == slug
                    select new ProductDetailProjection(
                        p.Id.Value,
                        p.Title,
                        p.Slug.Value,
                        p.Sku.Value,
                        p.Price.Amount,
                        p.Price.Currency,
                        p.CategoryId.Value,
                        category.Name,
                        stock == null ? 0 : stock.Available,
                        p.Description))
                    .SingleOrDefaultAsync(ct);

                if (product is null)
                {
                    return Results.NotFound();
                }

                ReviewSummary ratings = await db.Reviews
                    .AsNoTracking()
                    .Where(review => review.ProductId.Value == product.Id && review.Status == ReviewStatus.Approved)
                    .GroupBy(_ => 1)
                    .Select(group => new ReviewSummary(
                        group.Average(review => (double)review.Rating),
                        group.Count()))
                    .SingleOrDefaultAsync(ct)
                    ?? new ReviewSummary(null, 0);

                ProductDetailResponse response = new(
                    product.Id,
                    product.Title,
                    product.Slug,
                    product.Sku,
                    product.Price,
                    product.Currency,
                    product.CategoryId,
                    product.Category,
                    [],
                    product.Description,
                    product.Stock,
                    [new ProductAttribute("Category", product.Category)],
                    "Shipping options and delivery estimates are confirmed at checkout.",
                    ratings.AverageRating,
                    ratings.RatingCount);

                return Results.Ok(response);
            })
            .WithName("GetProductDetail")
            .WithFeature("Catalog")
            .WithSummary("Get product detail")
            .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed record ProductDetailResponse(
    Guid Id,
    string Title,
    string Slug,
    string Sku,
    decimal Price,
    string Currency,
    Guid CategoryId,
    string Category,
    IReadOnlyList<string> Images,
    string LongDescription,
    int Stock,
    IReadOnlyList<ProductAttribute> Attributes,
    string ShippingInfo,
    double? AverageRating,
    int RatingCount);

public sealed record ProductAttribute(string Name, string Value);

internal sealed record ProductDetailProjection(
    Guid Id,
    string Title,
    string Slug,
    string Sku,
    decimal Price,
    string Currency,
    Guid CategoryId,
    string Category,
    int Stock,
    string Description);

internal sealed record ReviewSummary(double? AverageRating, int RatingCount);
