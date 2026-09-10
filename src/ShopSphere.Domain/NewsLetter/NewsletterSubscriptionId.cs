namespace ShopSphere.Domain.Newsletter;
public readonly record struct NewsletterSubscriptionId(Guid Value)
{
    public static NewsletterSubscriptionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}
