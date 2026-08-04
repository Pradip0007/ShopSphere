using MediatR;
using ShopSphere.Api.Features.Catalog.ListProducts;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.SearchProducts;

public sealed record SearchProductsQuery(
    string Q,
    int Page = 1,
    int PageSize = 20)
    : IRequest<PagedResult<ProductListItem>>;