using FluentValidation;

namespace ShopSphere.Api.Features.Auth.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(128);
    }
}