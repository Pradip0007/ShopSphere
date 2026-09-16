using MediatR;

namespace ShopSphere.Api.Features.Admin.PublishProduct;

public sealed record PublishProductCommand(Guid Id) : IRequest;
