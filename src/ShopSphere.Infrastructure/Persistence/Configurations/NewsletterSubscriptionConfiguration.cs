using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Newsletter;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public sealed class NewsletterSubscriptionConfiguration
    : IEntityTypeConfiguration<NewsletterSubscription>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscription> b)
    {
        b.ToTable("NewsletterSubscriptions");

        b.HasKey(subscription => subscription.Id);

        b.Property(subscription => subscription.Id)
            .HasConversion(
                id => id.Value,
                value => new NewsletterSubscriptionId(value));

        b.Property(subscription => subscription.Email)
            .HasMaxLength(320)
            .IsRequired();

        b.Property(subscription => subscription.SubscribedAtUtc)
            .IsRequired();

        b.HasIndex(subscription => subscription.Email)
            .IsUnique();
    }
}
