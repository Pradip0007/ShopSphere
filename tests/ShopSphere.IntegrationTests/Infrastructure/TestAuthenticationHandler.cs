using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ShopSphere.IntegrationTests.Infrastructure;

public sealed class TestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string HeaderUserId = "X-Test-UserId";
    public const string HeaderRoles = "X-Test-Roles";
    public const string HeaderPermissions = "X-Test-Permissions";
    public const string HeaderEmail = "X-Test-Email";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderUserId, out var uid))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, uid.ToString()),
            new("sub", uid.ToString()),
        };

        if (Request.Headers.TryGetValue(HeaderEmail, out var email))
            claims.Add(new Claim(ClaimTypes.Email, email.ToString()));

        if (Request.Headers.TryGetValue(HeaderRoles, out var roles))
        {
            foreach (var r in roles.ToString().Split(','))
            {
                if (!string.IsNullOrWhiteSpace(r))
                    claims.Add(new Claim(ClaimTypes.Role, r.Trim()));
            }
        }

        if (Request.Headers.TryGetValue(HeaderPermissions, out var permissions))
        {
            foreach (var permission in permissions.ToString().Split(','))
            {
                if (!string.IsNullOrWhiteSpace(permission))
                    claims.Add(new Claim("permission", permission.Trim()));
            }
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
