using MediatR;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.ListProducts;

public sealed record ListProductsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Sort = null)
    : IRequest<PagedResult<ProductListItem>>;

public sealed record ProductListItem(
    Guid Id,
    string Title,
    string Slug,
    string Sku,
    decimal Price,
    string Currency,
    Guid CategoryId);