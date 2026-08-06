using FluentValidation;

namespace ShopSphere.Api.Features.Auth.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(c => c.Email).NotEmpty().MaximumLength(320);
        RuleFor(c => c.Password).NotEmpty().MaximumLength(128);
    }
}