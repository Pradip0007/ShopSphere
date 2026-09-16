using System.Security.Cryptography;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ShopSphere.Api.Auth;
using ShopSphere.Api.Features.Auth.Login;
using ShopSphere.Api.Features.Auth.Refresh;
using ShopSphere.Api.Features.Cart;
using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.UnitTests.Common;

namespace ShopSphere.UnitTests.Auth;

public sealed class AuthHandlerTests
{
    [Fact]
    public async Task Login_should_issue_tokens_and_persist_refresh_token()
    {
        await using var db = InMemoryDb.New();
        var hasher = new TestPasswordHasher();
        var user = User.Register("user@example.com", "StrongPassword1", hasher);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var time = new FixedTimeProvider(DateTimeOffset.Parse("2026-09-16T10:00:00Z", CultureInfo.InvariantCulture));
        var tokens = new TestTokenService(time.GetUtcNow());

        var response = await CreateLoginHandler(db, hasher, tokens, time).Handle(
            new LoginCommand(" USER@EXAMPLE.COM ", "StrongPassword1"), CancellationToken.None);

        response.AccessToken.Should().Be("access-token");
        response.RefreshToken.Should().Be("refresh-token");
        response.TokenType.Should().Be("Bearer");
        (await db.RefreshTokens.SingleAsync()).TokenHash.Should().Be("refresh-hash");
    }

    [Fact]
    public async Task Login_should_reject_unknown_user_and_wrong_password()
    {
        await using var db = InMemoryDb.New();
        var hasher = new TestPasswordHasher();
        var handler = CreateLoginHandler(db, hasher, new TestTokenService(DateTimeOffset.UtcNow), new FixedTimeProvider(DateTimeOffset.UtcNow));

        var unknown = () => handler.Handle(new LoginCommand("missing@example.com", "StrongPassword1"), CancellationToken.None);
        await unknown.Should().ThrowAsync<UnauthorizedAccessException>();

        var user = User.Register("user@example.com", "StrongPassword1", hasher);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var wrong = () => handler.Handle(new LoginCommand("user@example.com", "WrongPassword1"), CancellationToken.None);
        await wrong.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_should_reject_locked_user()
    {
        await using var db = InMemoryDb.New();
        var hasher = new TestPasswordHasher();
        var user = User.Register("locked@example.com", "StrongPassword1", hasher);
        user.LockOut();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = CreateLoginHandler(db, hasher, new TestTokenService(DateTimeOffset.UtcNow), new FixedTimeProvider(DateTimeOffset.UtcNow));
        var act = () => handler.Handle(new LoginCommand("locked@example.com", "StrongPassword1"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_should_merge_guest_cart_when_session_cookie_is_present()
    {
        await using var db = InMemoryDb.New();
        var hasher = new TestPasswordHasher();
        var user = User.Register("user@example.com", "StrongPassword1", hasher);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var carts = new Mock<ICartRepository>();
        var sessionId = Guid.NewGuid();
        var http = new DefaultHttpContext();
        http.Request.Headers.Cookie = $"{CartKeyResolver.SessionCookieName}={sessionId:D}";
        var time = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var handler = new LoginHandler(
            db,
            hasher,
            new TestTokenService(time.GetUtcNow()),
            time,
            carts.Object,
            new HttpContextAccessor { HttpContext = http });

        await handler.Handle(new LoginCommand("user@example.com", "StrongPassword1"), CancellationToken.None);

        carts.Verify(c => c.MergeAsync(
            CartKey.Session(sessionId),
            CartKey.User(user.Id.Value),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Refresh_should_rotate_existing_token()
    {
        await using var db = InMemoryDb.New();
        var now = DateTimeOffset.Parse("2026-09-16T10:00:00Z", CultureInfo.InvariantCulture);
        var user = User.Register("user@example.com", "StrongPassword1", new TestPasswordHasher());
        var existing = RefreshToken.IssueForNewFamily(user.Id, Hash("raw-refresh"), now, TimeSpan.FromHours(1));
        db.Users.Add(user);
        db.RefreshTokens.Add(existing);
        await db.SaveChangesAsync();
        var tokenService = new TestTokenService(now);
        var handler = new RefreshHandler(db, tokenService, new FixedTimeProvider(now), NullLogger<RefreshHandler>.Instance);

        var response = await handler.Handle(
            new RefreshCommand(Convert.ToBase64String("raw-refresh"u8.ToArray())), CancellationToken.None);

        response.AccessToken.Should().Be("access-token");
        response.RefreshToken.Should().Be("refresh-token");
        existing.RevokedAt.Should().Be(now);
        (await db.RefreshTokens.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Refresh_should_reject_malformed_unknown_expired_and_locked_tokens()
    {
        await using var db = InMemoryDb.New();
        var now = DateTimeOffset.Parse("2026-09-16T10:00:00Z", CultureInfo.InvariantCulture);
        var handler = new RefreshHandler(db, new TestTokenService(now), new FixedTimeProvider(now), NullLogger<RefreshHandler>.Instance);

        var malformed = () => handler.Handle(new RefreshCommand("not-base64"), CancellationToken.None);
        var unknown = () => handler.Handle(new RefreshCommand(Convert.ToBase64String("unknown"u8.ToArray())), CancellationToken.None);
        await malformed.Should().ThrowAsync<UnauthorizedAccessException>();
        await unknown.Should().ThrowAsync<UnauthorizedAccessException>();

        var user = User.Register("user@example.com", "StrongPassword1", new TestPasswordHasher());
        var expired = RefreshToken.IssueForNewFamily(user.Id, Hash("expired"), now.AddHours(-2), TimeSpan.FromHours(1));
        db.Users.Add(user);
        db.RefreshTokens.Add(expired);
        await db.SaveChangesAsync();
        var expiredAct = () => handler.Handle(new RefreshCommand(Convert.ToBase64String("expired"u8.ToArray())), CancellationToken.None);
        await expiredAct.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task Refresh_should_revoke_active_family_when_revoked_token_is_reused()
    {
        await using var db = InMemoryDb.New();
        var now = DateTimeOffset.Parse("2026-09-16T10:00:00Z", CultureInfo.InvariantCulture);
        var user = User.Register("user@example.com", "StrongPassword1", new TestPasswordHasher());
        var reused = RefreshToken.IssueForNewFamily(user.Id, Hash("reused"), now, TimeSpan.FromHours(1));
        reused.Revoke(now.AddMinutes(-1));
        var active = reused.IssueRotation(Hash("active"), now, TimeSpan.FromHours(1));
        db.Users.Add(user);
        db.RefreshTokens.AddRange(reused, active);
        await db.SaveChangesAsync();
        var handler = new RefreshHandler(db, new TestTokenService(now), new FixedTimeProvider(now), NullLogger<RefreshHandler>.Instance);

        var act = () => handler.Handle(new RefreshCommand(Convert.ToBase64String("reused"u8.ToArray())), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*reuse detected*");
        (await db.RefreshTokens.SingleAsync(t => t.Id == active.Id)).IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Refresh_should_reject_missing_and_locked_users()
    {
        await using var db = InMemoryDb.New();
        var now = DateTimeOffset.UtcNow;
        var missingUserId = UserId.New();
        var missing = RefreshToken.IssueForNewFamily(missingUserId, Hash("missing"), now, TimeSpan.FromHours(1));
        db.RefreshTokens.Add(missing);
        await db.SaveChangesAsync();
        var handler = new RefreshHandler(db, new TestTokenService(now), new FixedTimeProvider(now), NullLogger<RefreshHandler>.Instance);

        var missingAct = () => handler.Handle(new RefreshCommand(Convert.ToBase64String("missing"u8.ToArray())), CancellationToken.None);
        await missingAct.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*User not found*");

        var user = User.Register("locked@example.com", "StrongPassword1", new TestPasswordHasher());
        user.LockOut();
        var locked = RefreshToken.IssueForNewFamily(user.Id, Hash("locked"), now, TimeSpan.FromHours(1));
        db.Users.Add(user);
        db.RefreshTokens.Add(locked);
        await db.SaveChangesAsync();

        var lockedAct = () => handler.Handle(new RefreshCommand(Convert.ToBase64String("locked"u8.ToArray())), CancellationToken.None);
        await lockedAct.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*Account locked*");
    }

    private static LoginHandler CreateLoginHandler(
        ShopSphereDbContext db,
        IPasswordHasher hasher,
        ITokenService tokens,
        TimeProvider time)
    {
        var carts = new Mock<ICartRepository>();
        carts.Setup(c => c.MergeAsync(It.IsAny<CartKey>(), It.IsAny<CartKey>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new LoginHandler(
            db,
            hasher,
            tokens,
            time,
            carts.Object,
            new HttpContextAccessor());
    }

    private static string Hash(string value) => Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public string Hash(string plainPassword) => $"hash:{plainPassword}";
        public bool Verify(string plainPassword, string storedHash) => storedHash == $"hash:{plainPassword}";
    }

    private sealed class TestTokenService(DateTimeOffset now) : ITokenService
    {
        public IssuedToken IssueAccessToken(User user) => new("access-token", now.AddMinutes(15));
        public IssuedRefreshToken IssueRefreshToken() => new("refresh-token", "refresh-hash", now.AddDays(7));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
