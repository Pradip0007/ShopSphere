using System.Text.RegularExpressions;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Users;

public sealed partial class User : AggregateRoot<UserId>
{
    // EF Core needs a parameterless constructor.
    // Domain code must use Register().
    private User()
    {
    }

    private User(UserId id) : base(id)
    {
    }

    public string Email { get; private set; } = default!;

    public string DisplayName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = default!;

    public DateTimeOffset RegisteredAt { get; private set; }

    public bool IsLockedOut { get; private set; }

    private readonly List<Role> _roles = [];

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    public static User Register(
        string email,
        string plainPassword,
        IPasswordHasher hasher,
        string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(plainPassword);
        ArgumentNullException.ThrowIfNull(hasher);

        string normalized = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Email format is invalid.",
                nameof(email));
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

        if (!HasUpper(plainPassword)
            || !HasLower(plainPassword)
            || !HasDigit(plainPassword))
        {
            throw new ArgumentException(
                "Password must contain upper, lower, and digit characters.",
                nameof(plainPassword));
        }

        string normalizedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? normalized.Split('@')[0]
            : displayName.Trim();

        if (normalizedDisplayName.Length > 60)
        {
            throw new ArgumentException(
                "Display name must be at most 60 characters.",
                nameof(displayName));
        }

        User user = new(UserId.New())
        {
            Email = normalized,
            DisplayName = normalizedDisplayName,
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

    private static bool HasUpper(string value) =>
        value.Any(char.IsUpper);

    private static bool HasLower(string value) =>
        value.Any(char.IsLower);

    private static bool HasDigit(string value) =>
        value.Any(char.IsDigit);

    [GeneratedRegex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    public bool VerifyPassword(
        string plainPassword,
        IPasswordHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        if (IsLockedOut)
        {
            return false;
        }

        return hasher.Verify(
            plainPassword ?? string.Empty,
            PasswordHash);
    }

    public void ResetPassword(
    string newPassword,
    IPasswordHasher hasher)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);
        ArgumentNullException.ThrowIfNull(hasher);

        if (newPassword.Length < 12)
        {
            throw new ArgumentException(
                "Password must be at least 12 characters.",
                nameof(newPassword));
        }

        if (newPassword.Length > 128)
        {
            throw new ArgumentException(
                "Password must be at most 128 characters.",
                nameof(newPassword));
        }

        if (!HasUpper(newPassword)
            || !HasLower(newPassword)
            || !HasDigit(newPassword))
        {
            throw new ArgumentException(
                "Password must contain upper, lower, and digit characters.",
                nameof(newPassword));
        }

        PasswordHash = hasher.Hash(newPassword);
        IsLockedOut = false;
    }

    public void AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (_roles.Any(r => r.Id == role.Id))
        {
            return;
        }

        _roles.Add(role);
    }

    public void RemoveRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Role? existing = _roles
            .FirstOrDefault(r => r.Id == role.Id);

        if (existing is not null)
        {
            _roles.Remove(existing);
        }
    }

    public Result ChangeDisplayName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(
                new Error(
                    "user.display_name.invalid",
                    "Display name is required."));
        }

        string normalizedName = name.Trim();

        if (normalizedName.Length > 60)
        {
            return Result.Failure(
                new Error(
                    "user.display_name.invalid",
                    "Display name must be at most 60 characters."));
        }

        DisplayName = normalizedName;

        return Result.Success();
    }

    public IEnumerable<string> EffectivePermissions() =>
        _roles
            .SelectMany(r => r.Permissions)
            .Select(p => p.Name)
            .Distinct(StringComparer.Ordinal);
}