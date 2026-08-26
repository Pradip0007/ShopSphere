using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Infrastructure.Outbox;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("OutboxMessages");
        b.HasKey(message => message.Id);
        b.Property(message => message.Type).HasMaxLength(512).IsRequired();
        b.Property(message => message.PayloadJson).IsRequired();
        b.Property(message => message.LastError).HasMaxLength(1000);
        b.Property(message => message.ClaimedBy).HasMaxLength(128);
        b.HasIndex(message => new { message.ProcessedAtUtc, message.ClaimedAtUtc })
            .HasDatabaseName("IX_OutboxMessages_Pending")
            .HasFilter("[ProcessedAtUtc] IS NULL");
    }
}
