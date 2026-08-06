using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Users;

public sealed class RefreshToken : AggregateRoot<RefreshTokenId>
{
    // EF Core
    private RefreshToken() { }

    private RefreshToken(RefreshTokenId id) : base(id)
    {
    }

    public UserId UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public Guid Family { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public RefreshTokenId? ReplacedByTokenId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static RefreshToken IssueForNewFamily(
        UserId userId,
        string tokenHash,
        DateTimeOffset now,
        TimeSpan lifetime)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);

        return new RefreshToken(RefreshTokenId.New())
        {
            UserId = userId,
            TokenHash = tokenHash,
            Family = Guid.NewGuid(),
            CreatedAt = now,
            ExpiresAt = now + lifetime
        };
    }

    public RefreshToken IssueRotation(
        string tokenHash,
        DateTimeOffset now,
        TimeSpan lifetime)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);

        return new RefreshToken(RefreshTokenId.New())
        {
            UserId = UserId,
            TokenHash = tokenHash,
            Family = Family,
            CreatedAt = now,
            ExpiresAt = now + lifetime
        };
    }

    public void MarkReplaced(
        RefreshTokenId nextTokenId,
        DateTimeOffset now)
    {
        RevokedAt = now;
        ReplacedByTokenId = nextTokenId;
    }

    public void Revoke(DateTimeOffset now)
    {
        RevokedAt = now;
    }

    public bool IsActive(DateTimeOffset now)
        => RevokedAt is null && ExpiresAt > now;

    public bool IsRevoked
        => RevokedAt is not null;

    public bool IsExpired(DateTimeOffset now)
        => ExpiresAt <= now;
}