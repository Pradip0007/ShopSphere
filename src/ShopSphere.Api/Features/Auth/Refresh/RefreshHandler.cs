using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Auth;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Auth.Refresh;

public sealed class RefreshHandler(
    ShopSphereDbContext db,
    ITokenService tokens,
    TimeProvider timeProvider,
    ILogger<RefreshHandler> logger)
    : IRequestHandler<RefreshCommand, RefreshResponse>
{
    public async Task<RefreshResponse> Handle(
        RefreshCommand request,
        CancellationToken cancellationToken)
    {
        byte[] rawBytes;
        try
        {
            rawBytes = Convert.FromBase64String(request.RefreshToken);
        }
        catch (FormatException)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        string hash = Convert.ToBase64String(SHA256.HashData(rawBytes));

        RefreshToken? existing = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        DateTimeOffset now = timeProvider.GetUtcNow();

        if (existing is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        // REUSE DETECTION: an already-revoked or replaced token was presented.
        if (existing.IsRevoked)
        {
            logger.LogWarning(
                "Refresh token reuse detected for user {UserId}, family {Family}. Revoking entire family.",
                existing.UserId,
                existing.Family);

            List<RefreshToken> family = await db.RefreshTokens
                .Where(t => t.Family == existing.Family && t.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (RefreshToken t in family)
            {
                t.Revoke(now);
            }
            await db.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("Refresh token reuse detected.");
        }

        if (existing.IsExpired(now))
        {
            throw new UnauthorizedAccessException("Refresh token expired.");
        }

        User user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == existing.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (user.IsLockedOut)
        {
            throw new UnauthorizedAccessException("Account locked.");
        }

        // Issue new pair, rotate.
        IssuedToken newAccess = tokens.IssueAccessToken(user);
        IssuedRefreshToken newRefresh = tokens.IssueRefreshToken();

        RefreshToken rotated = existing.IssueRotation(
            newRefresh.Hash,
            now,
            newRefresh.ExpiresAt - now);

        existing.MarkReplaced(rotated.Id, now);
        db.RefreshTokens.Add(rotated);
        await db.SaveChangesAsync(cancellationToken);

        return new RefreshResponse(
            newAccess.Value,
            newAccess.ExpiresAt,
            newRefresh.Value,
            newRefresh.ExpiresAt);
    }
}
