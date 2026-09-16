using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Features.Newsletter.Subscribe;
using ShopSphere.Api.Middleware;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.UnitTests.Common;

namespace ShopSphere.UnitTests.Newsletter;

public sealed class SubscribeHandlerTests
{
    [Fact]
    public async Task Subscribe_should_normalize_and_persist_email()
    {
        await using var db = InMemoryDb.New();

        var result = await new SubscribeHandler(db).Handle(
            new SubscribeCommand("  USER@Example.com "), CancellationToken.None);

        result.Message.Should().Contain("check your inbox");
        (await db.NewsletterSubscriptions.SingleAsync()).Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Subscribe_should_reject_duplicate_email_case_insensitively()
    {
        await using var db = InMemoryDb.New();
        await new SubscribeHandler(db).Handle(
            new SubscribeCommand("user@example.com"), CancellationToken.None);

        var act = () => new SubscribeHandler(db).Handle(
            new SubscribeCommand(" USER@EXAMPLE.COM "), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already subscribed*");
    }
}
