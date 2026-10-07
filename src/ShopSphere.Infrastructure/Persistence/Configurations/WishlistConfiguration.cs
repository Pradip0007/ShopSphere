using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Wishlist;
using ShopSphere.Domain.Users;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public sealed class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasConversion(
                id => id.Value,
                value => new WishlistId(value));

        builder.Property(w => w.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        builder.HasIndex(w => w.UserId)
            .IsUnique();

        builder.OwnsMany(w => w.Items, item =>
        {
            item.ToTable("WishlistItems");

            item.WithOwner()
                .HasForeignKey("WishlistId");

            item.HasKey("WishlistId", "ProductId");

            item.Property(x => x.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => new ProductId(value));

            item.Property(x => x.AddedAtUtc)
                .IsRequired();
        });

        builder.Ignore(w => w.DomainEvents);
    }
}