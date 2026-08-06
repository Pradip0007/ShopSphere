using System.Text.RegularExpressions;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Users;

public sealed partial class User : AggregateRoot<UserId>
{
    // EF Core needs a parameterless ctor. Domain code must use Register().
    private User() { }

    private User(UserId id) : base(id)
    {
    }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public DateTimeOffset RegisteredAt { get; private set; }
    public bool IsLockedOut { get; private set; }

    public static User Register(string email, string plainPassword, IPasswordHasher hasher)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(plainPassword);
        ArgumentNullException.ThrowIfNull(hasher);

        string normalized = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
        {
            throw new ArgumentException("Email format is invalid.", nameof(email));
        }

        if (plainPassword.Length < 12)
        {
            throw new ArgumentException(
                "Password must be at least 12 characters.",
                nameof(plainPassword));
        }
        if (plainPassword.Length > 128)
        {
            throw new ArgumentException(
                "Password must be at most 128 characters.",
                nameof(plainPassword));
        }
        if (!HasUpper(plainPassword) || !HasLower(plainPassword) || !HasDigit(plainPassword))
        {
            throw new ArgumentException(
                "Password must contain upper, lower, and digit characters.",
                nameof(plainPassword));
        }

        User user = new(UserId.New())
        {
            Email = normalized,
            PasswordHash = hasher.Hash(plainPassword),
            RegisteredAt = DateTimeOffset.UtcNow,
            IsLockedOut = false,
        };
user.Raise(
    new UserRegisteredEvent(
        user.Id,
        user.Email,
        user.RegisteredAt));
        return user;
    }

    public void LockOut() => IsLockedOut = true;
    public void Unlock() => IsLockedOut = false;

    private static bool HasUpper(string s) => s.Any(char.IsUpper);
    private static bool HasLower(string s) => s.Any(char.IsLower);
    private static bool HasDigit(string s) => s.Any(char.IsDigit);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}