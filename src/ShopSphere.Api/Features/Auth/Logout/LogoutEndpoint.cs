using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Api.Features.Auth.Refresh;

namespace ShopSphere.Api.Features.Auth.Logout;

public sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", (HttpContext http) =>
            {
                http.Response.Cookies.Delete(
                    RefreshEndpoint.CookieName,
                    new CookieOptions
                    {
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Path = "/api/v1/auth",
                    });

                return Results.NoContent();
            })
            .WithName("Logout")
            .WithFeature("Auth")
            .WithSummary("Sign out and clear the refresh cookie")
            .Produces(StatusCodes.Status204NoContent)
            .AllowAnonymous();
    }
}