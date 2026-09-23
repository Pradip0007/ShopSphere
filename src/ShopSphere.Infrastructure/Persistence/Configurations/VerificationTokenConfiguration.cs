using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Users;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public sealed class VerificationTokenConfiguration
    : IEntityTypeConfiguration<VerificationToken>
{
    public void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        builder.ToTable("VerificationTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => new VerificationTokenId(value));

        builder.Property(t => t.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        builder.Property(t => t.Kind)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.HashedToken)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(t => t.ExpiresAtUtc)
            .IsRequired();

        builder.Property(t => t.ConsumedAtUtc);

        builder.HasIndex(t => new { t.UserId, t.Kind });

        builder.HasIndex(t => t.HashedToken)
            .IsUnique();
    }
}