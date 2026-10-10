using System.Security.Claims;

namespace ShopSphere.Api.Features.Wishlist;

public static class WishlistSessionResolver
{
    public const string SessionCookieName = "wishlist_session";

    private static readonly CookieOptions SessionCookieOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        IsEssential = true,
        MaxAge = TimeSpan.FromDays(30),
        Path = "/"
    };

    public static Guid? From(HttpContext http)
    {
        ArgumentNullException.ThrowIfNull(http);

        var userIdClaim = http.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? http.User?.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out _))
        {
            return null;
        }

        if (http.Request.Cookies.TryGetValue(SessionCookieName, out var existing)
            && Guid.TryParse(existing, out var sessionGuid))
        {
            return sessionGuid;
        }

        var minted = Guid.NewGuid();

        http.Response.Cookies.Append(
            SessionCookieName,
            minted.ToString("D"),
            SessionCookieOptions);

        return minted;
    }
}