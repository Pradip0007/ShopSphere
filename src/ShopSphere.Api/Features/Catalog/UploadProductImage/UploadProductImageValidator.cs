using FluentValidation;

namespace ShopSphere.Api.Features.Catalog.UploadProductImage;

public sealed class UploadProductImageValidator
    : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Image file is required.");

        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .WithMessage("Image file cannot be empty.");

        RuleFor(x => x.File.ContentType)
            .Must(IsValidImageType)
            .WithMessage("Only JPEG, PNG, and WebP images are allowed.");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(5 * 1024 * 1024)
            .WithMessage("Image size cannot exceed 5 MB.");
    }

    private static bool IsValidImageType(string? contentType)
    {
        return contentType is
            "image/jpeg" or
            "image/png" or
            "image/webp";
    }
}