using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Infrastructure.Audit;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(log => log.Id);
        b.Property(log => log.EntityType).HasMaxLength(256).IsRequired();
        b.Property(log => log.EntityId).HasMaxLength(128).IsRequired();
        b.Property(log => log.Action).HasConversion<int>().IsRequired();
        b.Property(log => log.PayloadJson).IsRequired();
        b.Property(log => log.IpAddress).HasMaxLength(64);
        b.Property(log => log.UserAgent).HasMaxLength(512);
        b.HasIndex(log => new { log.EntityType, log.EntityId, log.TimestampUtc });
        b.HasIndex(log => log.TimestampUtc);
    }
}
