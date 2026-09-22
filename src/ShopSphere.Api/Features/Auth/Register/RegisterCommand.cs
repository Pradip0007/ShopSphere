using MediatR;

namespace ShopSphere.Api.Features.Auth.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string? DisplayName) : IRequest<RegisterResponse>;

public sealed record RegisterResponse(
    Guid UserId,
    string DisplayName);