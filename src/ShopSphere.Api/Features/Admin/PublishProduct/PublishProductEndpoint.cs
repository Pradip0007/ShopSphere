using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Users;

namespace ShopSphere.Api.Features.Admin.PublishProduct;

public sealed class PublishProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/products/{id:guid}/publish", async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                await sender.Send(new PublishProductCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("PublishProduct")
            .WithFeature("Admin.Catalog")
            .WithSummary("Publish a product")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(Permissions.ProductsWrite);
    }
}
