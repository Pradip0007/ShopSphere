using FluentValidation;

namespace ShopSphere.Api.Features.Wishlist;

public sealed class WishlistItemValidator : AbstractValidator<WishlistItemCommand>
{
    public WishlistItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");

        RuleFor(x => x.Action)
            .IsInEnum()
            .WithMessage("Invalid wishlist item action.");
    }
}