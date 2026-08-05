using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Admin.CreateProduct;

public sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/products", async (
                CreateProductCommand command,
                ISender sender,
                CancellationToken ct) =>
            {
                CreateProductResponse response = await sender.Send(command, ct);
                return Results.Created($"/api/v1/products/{response.Id}", response);
            })
            .WithName("CreateProduct")
            .WithFeature("Admin.Catalog")
            .WithSummary("Create a product")
            .WithDescription("Admin-only. Creates a new product in the given category. Role gate stubbed today; RBAC lands Day 29.")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        // TODO(Day 29): .RequireAuthorization("products.write");
    }
}