using System.Security.Claims;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Me;

public sealed class GetMeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/me", (ClaimsPrincipal user) =>
            {
                string? id = user.FindFirstValue("sub");
                string? email = user.FindFirstValue("email");
                return Results.Ok(new { id, email });
            })
            .WithName("GetMe")
            .WithFeature("Auth")
            .WithSummary("Return the current user's identity")
            .RequireAuthorization();
    }
}