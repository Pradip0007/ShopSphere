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
        where p.Status != ProductStatus.Archived
        orderby p.Title
        select new ProductListItem(
            p.Id.Value,
            p.Title,
            p.Slug.Value,
            p.Price.Amount,
            p.Price.Currency,
            s.Available);

    int totalCount = await query.CountAsync(cancellationToken);

    List<ProductListItem> items = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);

    return new PagedResult<ProductListItem>(
        items,
        request.Page,
        request.PageSize,
        totalCount);
}
}