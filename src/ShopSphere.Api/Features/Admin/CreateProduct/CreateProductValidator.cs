using FluentValidation;

namespace ShopSphere.Api.Features.Admin.CreateProduct;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Sku).NotEmpty().MaximumLength(32).Matches("^[A-Za-z0-9]+(-[A-Za-z0-9]+)*$")
                      .WithMessage("Sku must be uppercase with dashes.");
        RuleFor(c => c.Description).NotNull().MaximumLength(4000);
        RuleFor(c => c.Price).GreaterThan(0);
        RuleFor(c => c.Currency).NotEmpty().Length(3);
        RuleFor(c => c.InitialStock).GreaterThanOrEqualTo(0);
        RuleFor(c => c.CategoryId).NotEmpty();
    }
}