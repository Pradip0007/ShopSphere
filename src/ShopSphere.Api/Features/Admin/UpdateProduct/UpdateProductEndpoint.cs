using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Admin.UpdateProduct;

public sealed record UpdateProductBody(
    string? Title,
    string? Slug,
    string? Description,
    decimal? Price,
    string? Currency,
    int? StockOnHand);

public sealed class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/products/{id:guid}", async (
                Guid id,
                UpdateProductBody body,
                ISender sender,
                CancellationToken ct) =>
            {
                UpdateProductCommand command = new(
                    id,
                    body.Title,
                    body.Slug,
                    body.Description,
                    body.Price,
                    body.Currency,
                    body.StockOnHand);

                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithName("UpdateProduct")
            .WithFeature("Admin.Catalog")
            .WithSummary("Update a product (partial)")
            .WithDescription("Admin-only. Null fields are left unchanged.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
        // TODO(Day 29): .RequireAuthorization("products.write");
    }
}