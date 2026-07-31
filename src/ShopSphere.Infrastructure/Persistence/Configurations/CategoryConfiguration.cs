using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure.Persistence.Converters;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
               .HasConversion(StronglyTypedIdConverters.CategoryId)
               .ValueGeneratedNever();

        builder.Property(c => c.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(c => c.Slug)
               .HasConversion(CatalogValueObjectConverters.Slug)
               .HasMaxLength(220)
               .IsRequired();

        builder.HasIndex(c => c.Slug).IsUnique();

        builder.Property(c => c.ParentId)
               .HasConversion(StronglyTypedIdConverters.NullableCategoryId);

        // Self-referencing FK on ParentId.
        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(c => c.ParentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.DomainEvents);
    }
}