using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Auth.Refresh;

public sealed record RefreshBody(string? RefreshToken);

public sealed class RefreshEndpoint : IEndpoint
{
    public const string CookieName = "shopsphere_refresh";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async (
                RefreshBody? body,
                HttpContext http,
                ISender sender,
                CancellationToken ct) =>
            {
                // Prefer HttpOnly cookie for browser clients.
                string? candidate = http.Request.Cookies.TryGetValue(CookieName, out string? c)
                    ? c
                    : body?.RefreshToken;

                if (string.IsNullOrEmpty(candidate))
                {
                    return Results.Unauthorized();
                }

                RefreshResponse response = await sender.Send(new RefreshCommand(candidate), ct);

                // If the request came in via cookie, roll the cookie forward too.
                if (http.Request.Cookies.ContainsKey(CookieName))
                {
                    http.Response.Cookies.Append(CookieName, response.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = response.RefreshExpiresAt,
                        Path = "/api/v1/auth",
                    });
                }

                return Results.Ok(response);
            })
            .WithName("Refresh")
            .WithFeature("Auth")
            .WithSummary("Rotate refresh + access token pair")
            .WithDescription("Presents a refresh token; returns a new pair. If the presented token has already been rotated (reuse), the entire family is revoked.")
            .Produces<RefreshResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
    }
}