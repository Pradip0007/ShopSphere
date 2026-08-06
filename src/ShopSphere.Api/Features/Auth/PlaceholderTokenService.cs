using System.Security.Cryptography;
using ShopSphere.Domain.Users;

namespace ShopSphere.Api.Auth;

// NOTE: Placeholder for Day 26 only. Day 27 replaces with JwtTokenService.
public sealed class PlaceholderTokenService : ITokenService
{
    public IssuedToken IssueAccessToken(User user)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        string value = Convert.ToBase64String(bytes);
        return new IssuedToken(value, DateTimeOffset.UtcNow.AddMinutes(15));
    }
}