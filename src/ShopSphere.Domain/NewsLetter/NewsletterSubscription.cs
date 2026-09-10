namespace ShopSphere.Domain.Newsletter;

public sealed class NewsletterSubscription
{
    public NewsletterSubscriptionId Id { get; private set; }

    public string Email { get; private set; } = default!;

    public DateTimeOffset SubscribedAtUtc { get; private set; }

    private NewsletterSubscription()
    {
    }

    private NewsletterSubscription(
        NewsletterSubscriptionId id,
        string email)
    {
        Id = id;
        Email = email;
        SubscribedAtUtc = DateTimeOffset.UtcNow;
    }

    public static NewsletterSubscription Subscribe(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        return new NewsletterSubscription(
            NewsletterSubscriptionId.New(),
            email.Trim().ToLowerInvariant());
    }
}
