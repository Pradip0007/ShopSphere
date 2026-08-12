using System.Security.Claims;
using ShopSphere.Domain.Cart;

namespace ShopSphere.Api.Features.Cart;

public static class CartKeyResolver
{
    public const string SessionCookieName = "cart_session";

    private static readonly CookieOptions SessionCookieOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        IsEssential = true,
        MaxAge = TimeSpan.FromDays(30),
        Path = "/"
    };

    /// <summary>
    /// Returns the CartKey for the current caller. If the caller is authenticated
    /// (Day 24's JWT), a User(key) is returned. Otherwise a Session(key) is minted
    /// or read from the cart_session cookie; the cookie is written to the response
    /// when newly minted.
    /// </summary>
    public static CartKey From(HttpContext http)
    {
        ArgumentNullException.ThrowIfNull(http);

        var userIdClaim = http.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? http.User?.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userGuid))
        {
            return CartKey.User(userGuid);
        }

        if (http.Request.Cookies.TryGetValue(SessionCookieName, out var existing)
            && Guid.TryParse(existing, out var sessionGuid))
        {
            return CartKey.Session(sessionGuid);
        }

        var minted = Guid.NewGuid();
        http.Response.Cookies.Append(SessionCookieName, minted.ToString("D"), SessionCookieOptions);
        return CartKey.Session(minted);
    }
}