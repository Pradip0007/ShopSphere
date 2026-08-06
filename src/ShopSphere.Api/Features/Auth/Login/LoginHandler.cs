using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Auth;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.Infrastructure.Security;

namespace ShopSphere.Api.Features.Auth.Login;

public sealed class LoginHandler(
    ShopSphereDbContext db,
    IPasswordHasher hasher,
    ITokenService tokens)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    // Pre-computed once. Hash of "not-a-real-password".
    // We compare against this when the user doesn't exist, so total request
    // time is the same regardless of email validity.
    private static readonly Lazy<string> _dummyHash = new(() =>
        new Argon2PasswordHasher().Hash("not-a-real-password-9x9x"));

    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        string normalized = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

        User? user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        bool passwordOk;
        if (user is null)
        {
            // Burn the same cycles as a real verification so timing doesn't leak.
            _ = hasher.Verify(request.Password ?? string.Empty, _dummyHash.Value);
            passwordOk = false;
        }
        else
        {
            passwordOk = user.VerifyPassword(request.Password ?? string.Empty, hasher);
        }

        if (user is null || !passwordOk)
        {
            // Identical response for both failure modes.
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        IssuedToken token = tokens.IssueAccessToken(user);
        return new LoginResponse(token.Value, token.ExpiresAt);
    }
}