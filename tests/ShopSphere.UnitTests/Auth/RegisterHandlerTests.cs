using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Features.Auth.Register;
using ShopSphere.Api.Middleware;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.UnitTests.Common;

namespace ShopSphere.UnitTests.Auth;

public sealed class RegisterHandlerTests
{
    [Fact]
    public async Task Register_should_persist_user_with_customer_role()
    {
        await using var db = InMemoryDb.New();
        var role = Role.Create("customer");
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var result = await new RegisterHandler(db, new TestPasswordHasher()).Handle(
            new RegisterCommand(" USER@Example.com ", "StrongPassword1"), CancellationToken.None);

        var user = await db.Users.Include(u => u.Roles).SingleAsync();
        result.UserId.Should().Be(user.Id.Value);
        user.Email.Should().Be("user@example.com");
        user.PasswordHash.Should().Be("hash:StrongPassword1");
        user.Roles.Should().ContainSingle().Which.Name.Should().Be("customer");
    }

    [Fact]
    public async Task Register_should_reject_duplicate_email()
    {
        await using var db = InMemoryDb.New();
        var role = Role.Create("customer");
        db.Roles.Add(role);
        db.Users.Add(User.Register("user@example.com", "StrongPassword1", new TestPasswordHasher()));
        await db.SaveChangesAsync();

        var act = () => new RegisterHandler(db, new TestPasswordHasher()).Handle(
            new RegisterCommand(" USER@EXAMPLE.COM ", "StrongPassword1"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Registration failed.");
    }

    [Fact]
    public async Task Register_should_reject_when_customer_role_is_missing()
    {
        await using var db = InMemoryDb.New();

        var act = () => new RegisterHandler(db, new TestPasswordHasher()).Handle(
            new RegisterCommand("user@example.com", "StrongPassword1"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Customer role missing*");
    }

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public string Hash(string plainPassword) => $"hash:{plainPassword}";
        public bool Verify(string plainPassword, string storedHash) => storedHash == $"hash:{plainPassword}";
    }
}
