using MediatR;

namespace ShopSphere.Api.Features.Newsletter.Subscribe;

public sealed record SubscribeCommand(string Email) : IRequest<SubscribeResponse>;

public sealed record SubscribeResponse(string Message);
