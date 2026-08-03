using FluentValidation;

namespace ShopSphere.Api.Features.Catalog.ListProducts;

public sealed class ListProductsValidator : AbstractValidator<ListProductsQuery>
{
    public ListProductsValidator()
    {
        RuleFor(q => q.Page).GreaterThan(0);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);

        RuleFor(q => q.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(q => q.MinPrice.HasValue);

        RuleFor(q => q.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(q => q.MaxPrice.HasValue);

        RuleFor(q => q)
            .Must(q => !(q.MinPrice.HasValue && q.MaxPrice.HasValue) || q.MinPrice <= q.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice.")
            .WithName("PriceRange");

        RuleFor(q => q.Sort)
            .Must(s => s is null || ListProductsSort.Allowed.Contains(s))
            .WithMessage($"Sort must be one of: {string.Join(", ", ListProductsSort.Allowed)}.");
    }
}