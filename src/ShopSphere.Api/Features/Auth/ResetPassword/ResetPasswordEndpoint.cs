using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Auth.ResetPassword;

public sealed class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/reset-password", async (
                ResetPasswordCommand command,
                ISender sender,
                CancellationToken ct) =>
            {
                await sender.Send(command, ct);

                return Results.NoContent();
            })
            .WithName("ResetPassword")
            .WithFeature("Auth")
            .WithSummary("Reset the account password")
            .WithDescription(
                "Resets a user's password using a valid, single-use password reset token.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AllowAnonymous()
            .RequireRateLimiting("auth");
    }
}