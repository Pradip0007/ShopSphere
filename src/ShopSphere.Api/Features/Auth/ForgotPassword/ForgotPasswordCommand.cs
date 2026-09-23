using MediatR;

namespace ShopSphere.Api.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest;