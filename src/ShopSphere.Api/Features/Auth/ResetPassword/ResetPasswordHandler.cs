using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Auth.ResetPassword;

public sealed class ResetPasswordHandler(
    ShopSphereDbContext db,
    IPasswordHasher hasher,
    TimeProvider clock)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        List<VerificationToken> tokens =
            await db.VerificationTokens
                .Where(t =>
                    t.Kind == VerificationTokenKind.PasswordReset &&
                    t.ConsumedAtUtc == null &&
                    t.ExpiresAtUtc > clock.GetUtcNow())
                .ToListAsync(cancellationToken);

        VerificationToken? token = tokens
            .FirstOrDefault(t => t.Matches(request.Token));

        if (token is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired reset token.");
        }

        User? user = await db.Users
            .FirstOrDefaultAsync(
                u => u.Id == token.UserId,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired reset token.");
        }

        user.ResetPassword(request.NewPassword, hasher);

        if (!token.TryConsume(request.Token, clock))
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired reset token.");
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}