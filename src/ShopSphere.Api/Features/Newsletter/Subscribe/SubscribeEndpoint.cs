using MediatR;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Newsletter.Subscribe;

public sealed class SubscribeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/newsletter/subscribe", async (
                SubscribeCommand command,
                ISender sender,
                CancellationToken ct) =>
            {
                SubscribeResponse response = await sender.Send(command, ct);
                return Results.Ok(response);
            })
            .WithName("SubscribeToNewsletter")
            .WithFeature("Newsletter")
            .WithSummary("Subscribe to the newsletter")
            .WithDescription("Subscribes an email address to the newsletter.")
            .Produces<SubscribeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AllowAnonymous();
    }
}
