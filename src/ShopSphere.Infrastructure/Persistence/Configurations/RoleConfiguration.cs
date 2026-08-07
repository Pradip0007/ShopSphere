using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence.Converters;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration
    : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(StronglyTypedIdConverters.RoleId)
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder
            .HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity(j => j.ToTable("RolePermissions"));

        builder.Ignore(r => r.DomainEvents);
    }
}