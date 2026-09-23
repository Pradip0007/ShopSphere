using System.Security.Cryptography;
using System.Text;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Users;

public enum VerificationTokenKind
{
    PasswordReset = 2
}

public sealed class VerificationToken : AggregateRoot<VerificationTokenId>
{
    public UserId UserId { get; private set; }

    public VerificationTokenKind Kind { get; private set; }

    public string HashedToken { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? ConsumedAtUtc { get; private set; }

    private VerificationToken()
    {
        UserId = default;
    }

    private VerificationToken(
        VerificationTokenId id,
        UserId userId,
        VerificationTokenKind kind,
        string hashedToken,
        DateTimeOffset expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        Kind = kind;
        HashedToken = hashedToken;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static (VerificationToken Token, string PlainToken) Issue(
        UserId userId,
        VerificationTokenKind kind,
        TimeSpan lifetime)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        var plainToken = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var hash = Convert.ToHexString(
            SHA256.HashData(bytes));

        var token = new VerificationToken(
            VerificationTokenId.New(),
            userId,
            kind,
            hash,
            DateTimeOffset.UtcNow.Add(lifetime));

        return (token, plainToken);
    }

    public bool IsExpired(TimeProvider clock)
        => clock.GetUtcNow() > ExpiresAtUtc;

    public bool IsConsumed
        => ConsumedAtUtc is not null;

    public bool Matches(string plainToken)
    {
        byte[] tokenBytes;

        try
        {
            var padded = plainToken
                .Replace('-', '+')
                .Replace('_', '/');

            padded += new string(
                '=',
                (4 - padded.Length % 4) % 4);

            tokenBytes = Convert.FromBase64String(padded);
        }
        catch (FormatException)
        {
            return false;
        }

        var hash = SHA256.HashData(tokenBytes);

        byte[] storedHash;

        try
        {
            storedHash = Convert.FromHexString(HashedToken);
        }
        catch (FormatException)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            hash,
            storedHash);
    }

    public bool TryConsume(
        string plainToken,
        TimeProvider clock)
    {
        if (IsConsumed)
        {
            return false;
        }

        if (IsExpired(clock))
        {
            return false;
        }

        if (!Matches(plainToken))
        {
            return false;
        }

        ConsumedAtUtc = clock.GetUtcNow();

        return true;
    }
}