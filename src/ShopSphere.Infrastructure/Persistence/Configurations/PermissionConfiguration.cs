using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence.Converters;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration
    : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(StronglyTypedIdConverters.PermissionId)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Ignore(p => p.DomainEvents);
    }
}