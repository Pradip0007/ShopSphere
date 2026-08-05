using FluentValidation;

namespace ShopSphere.Api.Features.Admin.UpdateProduct;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(c => c.Id).NotEmpty();

        When(c => c.Title is not null, () =>
            RuleFor(c => c.Title!).NotEmpty().MaximumLength(200));

        When(c => c.Slug is not null, () =>
            RuleFor(c => c.Slug!).Matches("^[a-z0-9]+(-[a-z0-9]+)*$"));

        When(c => c.Description is not null, () =>
            RuleFor(c => c.Description!).MaximumLength(4000));

        When(c => c.Price.HasValue, () =>
            RuleFor(c => c.Price!.Value).GreaterThan(0));

        When(c => c.Currency is not null, () =>
            RuleFor(c => c.Currency!).Length(3));

        When(c => c.StockOnHand.HasValue, () =>
            RuleFor(c => c.StockOnHand!.Value).GreaterThanOrEqualTo(0));

        RuleFor(c => c)
            .Must(c => c.Price.HasValue == c.Currency is not null)
            .WithMessage("Price and Currency must be supplied together.")
            .WithName("Money");
    }
}