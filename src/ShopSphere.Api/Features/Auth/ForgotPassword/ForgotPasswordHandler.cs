using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Notifications;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordHandler(
    ShopSphereDbContext db,
    IEmailSender emailSender)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        User? user = await db.Users
            .FirstOrDefaultAsync(
                u => u.Email == email,
                cancellationToken);

        // Do not reveal whether the account exists.
        if (user is null)
        {
            return;
        }

        var issued = VerificationToken.Issue(
            user.Id,
            VerificationTokenKind.PasswordReset,
            TimeSpan.FromMinutes(15));

        db.Set<VerificationToken>().Add(issued.Token);

        await db.SaveChangesAsync(cancellationToken);

        var message = new EmailMessage(
            user.Email,
            user.DisplayName,
            "Reset your ShopSphere password",
            $"""
            Hello {user.DisplayName},

            We received a request to reset your ShopSphere password.

            Use the following token to reset your password:

            {issued.PlainToken}

            This token expires in 15 minutes and can only be used once.

            If you did not request this password reset, you can safely ignore this email.
            """,
            null);

        await emailSender.SendAsync(
            message,
            cancellationToken);
    }
}