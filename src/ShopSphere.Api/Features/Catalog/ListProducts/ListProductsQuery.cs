using MediatR;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.ListProducts;

public sealed record ListProductsQuery(int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<ProductListItem>>;

public sealed record ProductListItem(
    Guid Id,
    string Title,
    string Slug,
    decimal Price,
    string Currency,
    int StockOnHand);