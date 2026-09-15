using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Users;

namespace ShopSphere.UnitTests.Users;

public sealed class UserTests
{
    [Fact]
    public void Register_should_normalize_email_hash_password_and_raise_event()
    {
        var hasher = new TestPasswordHasher();

        var user = User.Register("  USER@Example.com ", "StrongPassword1", hasher);

        user.Email.Should().Be("user@example.com");
        user.PasswordHash.Should().Be("hash:StrongPassword1");
        user.IsLockedOut.Should().BeFalse();
        user.Roles.Should().BeEmpty();
        user.DomainEvents.Should().ContainSingle(e => e is UserRegisteredEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void Register_should_reject_invalid_email(string email)
    {
        var act = () => User.Register(email, "StrongPassword1", new TestPasswordHasher());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Short1")]
    [InlineData("alllowercase123")]
    [InlineData("ALLUPPERCASE123")]
    [InlineData("NoDigitsHere")]
    public void Register_should_reject_invalid_password(string password)
    {
        var act = () => User.Register("user@example.com", password, new TestPasswordHasher());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Register_should_reject_password_longer_than_128_characters()
    {
        var password = new string('A', 127) + "1a";

        var act = () => User.Register("user@example.com", password, new TestPasswordHasher());

        act.Should().Throw<ArgumentException>().WithMessage("*at most 128*");
    }

    [Fact]
    public void Register_should_reject_null_hasher()
    {
        var act = () => User.Register("user@example.com", "StrongPassword1", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VerifyPassword_should_delegate_to_hasher()
    {
        var hasher = new TestPasswordHasher();
        var user = User.Register("user@example.com", "StrongPassword1", hasher);

        user.VerifyPassword("StrongPassword1", hasher).Should().BeTrue();
        hasher.LastVerifiedHash.Should().Be("hash:StrongPassword1");
    }

    [Fact]
    public void VerifyPassword_should_return_false_when_locked_out()
    {
        var hasher = new TestPasswordHasher();
        var user = User.Register("user@example.com", "StrongPassword1", hasher);
        user.LockOut();

        user.VerifyPassword("StrongPassword1", hasher).Should().BeFalse();
        hasher.VerifyCalls.Should().Be(0);
    }

    [Fact]
    public void Unlock_should_allow_password_verification_again()
    {
        var hasher = new TestPasswordHasher();
        var user = User.Register("user@example.com", "StrongPassword1", hasher);
        user.LockOut();
        user.Unlock();

        user.VerifyPassword("StrongPassword1", hasher).Should().BeTrue();
    }

    [Fact]
    public void AssignRole_should_ignore_duplicate_role_and_effective_permissions_should_be_distinct()
    {
        var user = User.Register("user@example.com", "StrongPassword1", new TestPasswordHasher());
        var role = Role.Create("  manager ");
        var read = Permission.Create("products.read");
        var duplicateRead = Permission.Create("products.read");
        role.Grant(read);
        role.Grant(duplicateRead);

        user.AssignRole(role);
        user.AssignRole(role);

        user.Roles.Should().ContainSingle();
        user.EffectivePermissions().Should().Equal("products.read");
    }

    [Fact]
    public void RemoveRole_should_remove_existing_role_and_ignore_missing_role()
    {
        var user = User.Register("user@example.com", "StrongPassword1", new TestPasswordHasher());
        var role = Role.Create("manager");
        user.AssignRole(role);

        user.RemoveRole(role);
        user.RemoveRole(Role.Create("other"));

        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Role_should_trim_name_and_manage_permissions()
    {
        var role = Role.Create("  manager ");
        var permission = Permission.Create(" products.read ");

        role.Name.Should().Be("manager");
        permission.Name.Should().Be("products.read");
        role.Grant(permission);
        role.Grant(permission);
        role.Permissions.Should().ContainSingle().Which.Should().Be(permission);
        role.Revoke(permission);
        role.Revoke(permission);
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Permissions_should_expose_all_declared_permission_names()
    {
        Permissions.All.Should().BeEquivalentTo(
            Permissions.ProductsWrite,
            Permissions.ProductsRead,
            Permissions.OrdersReadSelf,
            Permissions.OrdersReadAll,
            Permissions.OrdersManage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Role_and_permission_should_reject_blank_names(string name)
    {
        var role = () => Role.Create(name);
        var permission = () => Permission.Create(name);

        role.Should().Throw<ArgumentException>();
        permission.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RefreshToken_should_issue_rotation_in_same_family()
    {
        var now = DateTimeOffset.UtcNow;
        var userId = UserId.New();
        var token = RefreshToken.IssueForNewFamily(userId, "hash-1", now, TimeSpan.FromHours(1));

        var rotated = token.IssueRotation("hash-2", now.AddMinutes(1), TimeSpan.FromHours(2));

        token.UserId.Should().Be(userId);
        token.ExpiresAt.Should().Be(now.AddHours(1));
        rotated.UserId.Should().Be(userId);
        rotated.Family.Should().Be(token.Family);
        rotated.ExpiresAt.Should().Be(now.AddMinutes(1).AddHours(2));
        token.IsActive(now).Should().BeTrue();
        token.IsExpired(now).Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_should_track_replacement_and_revocation()
    {
        var now = DateTimeOffset.UtcNow;
        var token = RefreshToken.IssueForNewFamily(UserId.New(), "hash", now, TimeSpan.FromHours(1));
        var nextId = RefreshTokenId.New();

        token.MarkReplaced(nextId, now.AddMinutes(2));

        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().Be(now.AddMinutes(2));
        token.ReplacedByTokenId.Should().Be(nextId);
        token.IsActive(now).Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_should_expire_at_expiry_boundary_and_reject_null_hash()
    {
        var now = DateTimeOffset.UtcNow;
        var token = RefreshToken.IssueForNewFamily(UserId.New(), "hash", now, TimeSpan.FromHours(1));

        token.IsExpired(now.AddHours(1)).Should().BeTrue();
        token.Revoke(now.AddMinutes(1));
        token.IsRevoked.Should().BeTrue();
        var act = () => RefreshToken.IssueForNewFamily(UserId.New(), null!, now, TimeSpan.Zero);

        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public int VerifyCalls { get; private set; }
        public string? LastVerifiedHash { get; private set; }

        public string Hash(string plainPassword) => $"hash:{plainPassword}";

        public bool Verify(string plainPassword, string storedHash)
        {
            VerifyCalls++;
            LastVerifiedHash = storedHash;
            return storedHash == $"hash:{plainPassword}";
        }
    }
}
