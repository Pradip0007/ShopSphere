using FluentValidation;

namespace ShopSphere.Api.Features.Catalog.ListProducts;

public sealed class ListProductsValidator : AbstractValidator<ListProductsQuery>
{
    public ListProductsValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}