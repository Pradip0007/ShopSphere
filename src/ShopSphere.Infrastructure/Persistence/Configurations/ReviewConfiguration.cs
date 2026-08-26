using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Reviews;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> b)
    {
        b.ToTable("Reviews");
        b.HasKey(review => review.Id);
        b.Property(review => review.Id)
            .HasConversion(id => id.Value, value => new ReviewId(value));
        b.Property(review => review.UserId).IsRequired();
        b.Property(review => review.ProductId)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .IsRequired();
        b.Property(review => review.Rating).IsRequired();
        b.Property(review => review.Body).HasMaxLength(4000).IsRequired();
        b.Property(review => review.Status).HasConversion<int>().IsRequired();
        b.Property(review => review.PostedAtUtc).IsRequired();
        b.Property(review => review.RejectionReason).HasMaxLength(1000);
        b.HasIndex(review => new { review.UserId, review.ProductId }).IsUnique();
        b.HasIndex(review => new { review.ProductId, review.Status });
    }
}
