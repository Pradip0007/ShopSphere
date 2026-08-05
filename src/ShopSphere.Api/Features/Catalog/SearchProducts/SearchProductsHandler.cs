using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Features.Catalog.ListProducts;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Catalog.SearchProducts;

// TODO(Day 85): Replace LIKE-based search with Semantic Kernel embeddings.
//               Introduce ProductEmbedding table, similarity function,
//               and a hybrid query (BM25 + cosine). See Phase 5 plan.
public sealed class SearchProductsHandler(ShopSphereDbContext db)
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductListItem>>
{
    public async Task<PagedResult<ProductListItem>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Escape SQL LIKE wildcards in the user's term.
        string term = request.Q
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);

        string pattern = $"%{term}%";

        IQueryable<Product> query = db.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Published)
            .Where(p => EF.Functions.Like(p.Title, pattern)
                     || EF.Functions.Like(p.Description, pattern))
            // Naive scoring: Title matches float to the top.
            .OrderByDescending(p => EF.Functions.Like(p.Title, pattern) ? 1 : 0)
            .ThenBy(p => p.Title);

        int totalCount = await query.CountAsync(cancellationToken);

        List<ProductListItem> items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListItem(
                p.Id.Value,
                p.Title,
                p.Slug.Value,
                p.Sku.Value,
                p.Price.Amount,
                p.Price.Currency,
                p.CategoryId.Value))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItem>(items, request.Page, request.PageSize, totalCount);
    }
}