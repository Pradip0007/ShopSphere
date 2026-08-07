using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Users;

namespace ShopSphere.Api.Features.Admin.DeleteProduct;

public sealed class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/products/{id:guid}", async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                await sender.Send(new DeleteProductCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteProduct")
            .WithFeature("Admin.Catalog")
            .WithSummary("Archive a product")
            .WithDescription("Admin-only. Soft delete: product row is retained with IsArchived=true.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(Permissions.ProductsWrite);
    }
}