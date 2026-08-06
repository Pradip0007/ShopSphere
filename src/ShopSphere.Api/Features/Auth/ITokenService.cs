using ShopSphere.Domain.Users;

namespace ShopSphere.Api.Auth;

public interface ITokenService
{
    IssuedToken IssueAccessToken(User user);
}

public sealed record IssuedToken(string Value, DateTimeOffset ExpiresAt);