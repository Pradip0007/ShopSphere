using MediatR;

namespace ShopSphere.Api.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<LoginResponse>;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string TokenType = "Bearer");