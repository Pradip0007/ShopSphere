using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forgot-password", async (
                ForgotPasswordCommand command,
                ISender sender,
                CancellationToken ct) =>
            {
                await sender.Send(command, ct);

                return Results.Accepted();
            })
            .WithName("ForgotPassword")
            .WithFeature("Auth")
            .WithSummary("Request a password reset")
            .WithDescription(
                "Sends a password reset email when the account exists. " +
                "The response is always 202 to avoid revealing whether an email is registered.")
            .Produces(StatusCodes.Status202Accepted)
            .AllowAnonymous()
            .RequireRateLimiting("auth");
    }
}
