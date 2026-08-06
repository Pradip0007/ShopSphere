using ShopSphere.Domain.Users;

namespace ShopSphere.Api.Auth;

public interface ITokenService
{
    IssuedToken IssueAccessToken(User user);
    IssuedRefreshToken IssueRefreshToken();

}

public sealed record IssuedToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>
/// A freshly-issued refresh token. Value is what the client sees;
/// Hash is what we store in the DB.
/// </summary>
public sealed record IssuedRefreshToken(string Value, string Hash, DateTimeOffset ExpiresAt);