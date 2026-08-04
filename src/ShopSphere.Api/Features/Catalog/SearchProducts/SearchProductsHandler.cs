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

        var query =
            from p in db.Products.AsNoTracking()
            join s in db.StockLevels.AsNoTracking()
                on p.Id equals s.ProductId
            join c in db.Categories.AsNoTracking()
                on p.CategoryId equals c.Id
            where p.Status != ProductStatus.Archived
            where EF.Functions.Like(p.Title, pattern)
               || EF.Functions.Like(p.Description, pattern)
            orderby EF.Functions.Like(p.Title, pattern) descending,
                    p.Title
            select new
            {
                Product = p,
                Stock = s,
                Category = c
            };

        int totalCount = await query.CountAsync(cancellationToken);

        List<ProductListItem> items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ProductListItem(
                x.Product.Id.Value,
                x.Product.Title,
                x.Product.Slug.Value,
                x.Product.Price.Amount,
                x.Product.Price.Currency,
                x.Stock.Available,
                x.Product.CategoryId.Value,
                x.Category.Name))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItem>(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }
}