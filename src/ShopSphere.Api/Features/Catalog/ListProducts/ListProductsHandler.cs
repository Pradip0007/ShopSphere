using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Catalog.ListProducts;

public sealed class ListProductsHandler(ShopSphereDbContext db)
    : IRequestHandler<ListProductsQuery, PagedResult<ProductListItem>>
{
    public async Task<PagedResult<ProductListItem>> Handle(
        ListProductsQuery request,
        CancellationToken ct)
    {

        IQueryable<Product> query = db.Products.AsNoTracking()
                                    .Where(p=>p.Status == ProductStatus.Published);

        // Category filter
        if (request.CategoryId is Guid categoryId)
        {
            query = query.Where(p => p.CategoryId == new CategoryId(categoryId));
        }

        // Minimum price filter
        if (request.MinPrice is decimal min)
        {
            query = query.Where(p => p.Price.Amount >= min);
        }

        // Maximum price filter
        if (request.MaxPrice is decimal max)
        {
            query = query.Where(p => p.Price.Amount <= max);
        }

        // Sorting
        query = request.Sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price.Amount),
            "price_desc" => query.OrderByDescending(p => p.Price.Amount),
            _            => query.OrderBy(p => p.Title)
        };

        int total = await query.CountAsync(ct);

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
            .ToListAsync(ct);

        return new PagedResult<ProductListItem>(
            items,
            request.Page,
            request.PageSize,
            total);
    }
}