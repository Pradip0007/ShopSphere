using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Auth;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.Infrastructure.Security;
using ShopSphere.Domain.Cart;
using ShopSphere.Api.Features.Cart;
using ShopSphere.Api.Features.Wishlist;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Wishlist;
using DomainWishlist = ShopSphere.Domain.Wishlist.Wishlist;

namespace ShopSphere.Api.Features.Auth.Login;

public sealed class LoginHandler(
    ShopSphereDbContext db,
    IPasswordHasher hasher,
    ITokenService tokens,
    TimeProvider timeProvider,
    ICartRepository carts,
    IHttpContextAccessor httpContextAccessor,
    GuestWishlistStore guestWishlist,
    IWishlistRepository wishlists)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    private static readonly Lazy<string> _dummyHash = new(() =>
        new Argon2PasswordHasher().Hash("not-a-real-password-9x9x"));

    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        string normalized = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

        User? user = await db.Users
        .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
        .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        bool passwordOk;
        
        if (user is null)
        {
            _ = hasher.Verify(request.Password ?? string.Empty, _dummyHash.Value);
            passwordOk = false;
        }
        else
        {
            passwordOk = user.VerifyPassword(request.Password ?? string.Empty, hasher);
        }

        if (user is null || !passwordOk)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Merge anonymous cart into the authenticated user's cart

        bool cartMerged = false;

        HttpContext? http = httpContextAccessor.HttpContext;

        if (http is not null)
        {
            cartMerged = await TryMergeGuestCartAsync(
                http,
                user.Id.Value,
                carts,
                cancellationToken);

            await TryMergeGuestWishlistAsync(
            http,
            user.Id,
            guestWishlist,
            wishlists,
            timeProvider,
            cancellationToken);
        }

        IssuedToken access = tokens.IssueAccessToken(user);
        IssuedRefreshToken refresh = tokens.IssueRefreshToken();

        RefreshToken entity = RefreshToken.IssueForNewFamily(
            user.Id,
            refresh.Hash,
            timeProvider.GetUtcNow(),
            refresh.ExpiresAt - timeProvider.GetUtcNow());

        db.RefreshTokens.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            access.Value,
            access.ExpiresAt,
            refresh.Value,
            refresh.ExpiresAt);
    }

    private static async Task<bool> TryMergeGuestCartAsync(
        HttpContext http,
        Guid userId,
        ICartRepository carts,
        CancellationToken ct)
    {
        if (!http.Request.Cookies.TryGetValue(
                CartKeyResolver.SessionCookieName,
                out var raw)
            || !Guid.TryParse(raw, out var sessionGuid))
        {
            return false;
        }

        var source = CartKey.Session(sessionGuid);

        var destination = CartKey.User(userId);

        await carts.MergeAsync(
            source,
            destination,
            ct);

        http.Response.Cookies.Delete(
            CartKeyResolver.SessionCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

        return true;
    }

    private static async Task TryMergeGuestWishlistAsync(
    HttpContext http,
    UserId userId,
    GuestWishlistStore guestWishlist,
    IWishlistRepository wishlists,
    TimeProvider clock,
    CancellationToken ct)
    {
        if (!http.Request.Cookies.TryGetValue(
                WishlistSessionResolver.SessionCookieName,
                out var raw)
            || !Guid.TryParse(raw, out var sessionGuid))
        {
            return;
        }

        IReadOnlyList<Guid> productIds =
            await guestWishlist.DrainAsync(sessionGuid);

        if (productIds.Count == 0)
        {
            http.Response.Cookies.Delete(
                WishlistSessionResolver.SessionCookieName,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });

            return;
        }

        DomainWishlist? wishlist =
            await wishlists.GetAsync(userId, ct);

        wishlist ??= DomainWishlist.For(userId);

        foreach (Guid productId in productIds)
        {
            wishlist.Add(
                new ProductId(productId),
                clock);
        }

        await wishlists.SaveAsync(wishlist, ct);

        http.Response.Cookies.Delete(
            WishlistSessionResolver.SessionCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
    }
}