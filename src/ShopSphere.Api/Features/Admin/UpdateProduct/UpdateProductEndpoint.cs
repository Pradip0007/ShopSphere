using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Users;

namespace ShopSphere.Api.Features.Admin.UpdateProduct;

public sealed record UpdateProductBody(
    string? Title,
    string? Description,
    decimal? Price,
    string? Currency);

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
                    body.Description,
                    body.Price,
                    body.Currency);

                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithName("UpdateProduct")
            .WithFeature("Admin.Catalog")
            .WithSummary("Update a product (partial)")
            .WithDescription("Admin-only. Null fields are left unchanged.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(Permissions.ProductsWrite);
    }
}