using MediatR;

namespace ShopSphere.Api.Features.Admin.CreateProduct;

public sealed record CreateProductCommand(
    string Title,
    string Slug,
    string Description,
    decimal Price,
    string Currency,
    int StockOnHand,
    Guid CategoryId)
    : IRequest<CreateProductResponse>;

public sealed record CreateProductResponse(Guid Id);