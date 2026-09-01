using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Api.Features.Auth.Refresh;

namespace ShopSphere.Api.Features.Auth.Login;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
                LoginCommand command,
                HttpContext http,
                ISender sender,
                CancellationToken ct) =>
            {
                LoginResponse response = await sender.Send(command, ct);

                http.Response.Cookies.Append(
                    RefreshEndpoint.CookieName,
                    response.RefreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = response.RefreshExpiresAt,
                        Path = "/api/v1/auth",
                    });

                return Results.Ok(response);
            })
            .WithName("Login")
            .WithFeature("Auth")
            .WithSummary("Sign in and receive an access token")
            .WithDescription("Returns a 15-minute access token. Refresh tokens land on Day 28.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
    }
}