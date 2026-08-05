using FluentValidation;

namespace ShopSphere.Api.Features.Admin.DeleteProduct;

public sealed class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}