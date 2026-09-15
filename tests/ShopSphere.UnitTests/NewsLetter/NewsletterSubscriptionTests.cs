using ShopSphere.Domain.Newsletter;

namespace ShopSphere.UnitTests.NewsLetter;

public sealed class NewsletterSubscriptionTests
{
    [Fact]
    public void Subscribe_should_normalize_email_and_set_identity_and_timestamp()
    {
        var before = DateTimeOffset.UtcNow;

        var subscription = NewsletterSubscription.Subscribe("  USER@Example.com ");

        subscription.Email.Should().Be("user@example.com");
        subscription.Id.Value.Should().NotBe(Guid.Empty);
        subscription.SubscribedAtUtc.Should().BeOnOrAfter(before);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Subscribe_should_reject_blank_email(string email)
    {
        var act = () => NewsletterSubscription.Subscribe(email);

        act.Should().Throw<ArgumentException>().WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Subscription_id_should_format_as_guid()
    {
        var id = NewsletterSubscriptionId.New();

        id.ToString().Should().Be(id.Value.ToString("D"));
    }
}
