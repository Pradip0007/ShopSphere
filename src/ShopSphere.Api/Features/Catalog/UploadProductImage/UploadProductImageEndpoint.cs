using MediatR;
using Microsoft.AspNetCore.Http;
using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Catalog.UploadProductImage;


public sealed class UploadProductImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
        "/products/{productId:guid}/images",
        async (
            Guid productId,
            IFormFile file,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UploadProductImageCommand(
                    productId,
                    file),
                cancellationToken);

            return Results.Created(
                $"/api/v1/products/{productId}/images",
                result);
        })
        .DisableAntiforgery()
        .WithName("UploadProductImage")
        .WithFeature("Catalog")
        .WithSummary("Upload product image")
        .WithDescription("Uploads an image for a product.")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<UploadProductImageResponse>(
            StatusCodes.Status201Created);
    }
}