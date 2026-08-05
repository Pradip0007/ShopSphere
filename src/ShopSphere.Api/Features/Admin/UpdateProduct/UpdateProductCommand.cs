using MediatR;

namespace ShopSphere.Api.Features.Admin.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string? Title,
    string? Slug,
    string? Description,
    decimal? Price,
    string? Currency,
    int? StockOnHand)
    : IRequest;