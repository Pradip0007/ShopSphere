using MediatR;

namespace ShopSphere.Api.Features.Auth.Refresh;

public sealed record RefreshCommand(string RefreshToken) : IRequest<RefreshResponse>;

public sealed record RefreshResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt,
    string TokenType = "Bearer");