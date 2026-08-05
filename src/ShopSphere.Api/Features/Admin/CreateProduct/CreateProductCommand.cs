using MediatR;

namespace ShopSphere.Api.Features.Admin.CreateProduct;

public sealed record CreateProductCommand(
    string Title,
    string Sku,
    string Description,
    decimal Price,
    string Currency,
    int InitialStock,
    Guid CategoryId)
    : IRequest<CreateProductResponse>;

public sealed record CreateProductResponse(Guid Id);