using MediatR;

namespace ShopSphere.Api.Features.Admin.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : IRequest;