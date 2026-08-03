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
        CancellationToken cancellationToken)
    {
        var query =
            from p in db.Products.AsNoTracking()
            join s in db.StockLevels.AsNoTracking()
                on p.Id equals s.ProductId
            join c in db.Categories.AsNoTracking()
                on p.CategoryId equals c.Id
            where p.Status != ProductStatus.Archived
            select new
            {
                Product = p,
                Stock = s,
                Category = c
            };

        // Category filter
        if (request.CategoryId is Guid categoryId)
        {
            query = query.Where(x => x.Product.CategoryId.Value == categoryId);
        }

        // Minimum price filter
        if (request.MinPrice is decimal minPrice)
        {
            query = query.Where(x => x.Product.Price.Amount >= minPrice);
        }

        // Maximum price filter
        if (request.MaxPrice is decimal maxPrice)
        {
            query = query.Where(x => x.Product.Price.Amount <= maxPrice);
        }

        // Sorting
        query = request.Sort switch
        {
            ListProductsSort.PriceAsc =>
                query.OrderBy(x => x.Product.Price.Amount),

            ListProductsSort.PriceDesc =>
                query.OrderByDescending(x => x.Product.Price.Amount),

            ListProductsSort.Name =>
                query.OrderBy(x => x.Product.Title),

            _ =>
                query.OrderBy(x => x.Product.Title),
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