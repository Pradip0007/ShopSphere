using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Auth.Register;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (
                RegisterCommand command,
                ISender sender,
                CancellationToken ct) =>
            {
                RegisterResponse response = await sender.Send(command, ct);
                return Results.Created($"/users/{response.UserId}", response);
            })
            .WithName("Register")
            .WithFeature("Auth")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account. Argon2id-hashed password. No PII beyond email is required.")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AllowAnonymous();
    }
}
