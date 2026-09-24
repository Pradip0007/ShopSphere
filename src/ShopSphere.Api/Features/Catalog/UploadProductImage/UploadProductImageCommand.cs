using MediatR;
using Microsoft.AspNetCore.Http;

namespace ShopSphere.Api.Features.Catalog.UploadProductImage;

public sealed record UploadProductImageCommand(
    Guid ProductId,
    IFormFile File
) : IRequest<UploadProductImageResponse>;