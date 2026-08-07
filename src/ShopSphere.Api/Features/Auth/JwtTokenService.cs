using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShopSphere.Api.Auth;
using ShopSphere.Domain.Users;

namespace ShopSphere.Api.Features.Auth;

public sealed class JwtTokenService(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider)
    : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public IssuedToken IssueAccessToken(User user)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset expires = now.AddMinutes(_options.AccessTokenLifetimeMinutes);

        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString("D")),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
        ];
        foreach (Role role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        foreach (string permission in user.EffectivePermissions())
        {
            claims.Add(new Claim("permission", permission));
        }

        SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(_options.Key));
        SigningCredentials credentials = new(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
        issuer: _options.Issuer,
        audience: _options.Audience,
        claims: claims,
        notBefore: now.UtcDateTime,
        expires: expires.UtcDateTime,
        signingCredentials: credentials);

        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
    
    public IssuedRefreshToken IssueRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        string value = Convert.ToBase64String(bytes);
        string hash = Convert.ToBase64String(SHA256.HashData(bytes));

        DateTimeOffset expires = timeProvider.GetUtcNow()
            .AddDays(_options.RefreshTokenLifetimeDays);

        return new IssuedRefreshToken(value, hash, expires);
    }
}