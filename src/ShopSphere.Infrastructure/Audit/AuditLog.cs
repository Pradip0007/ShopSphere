namespace ShopSphere.Infrastructure.Audit;

public enum AuditAction
{
    Create = 0,
    Update = 1,
    Delete = 2
}

public sealed class AuditLog
{
    public Guid Id { get; init; }
    public Guid? ActorUserId { get; init; }
    public string EntityType { get; init; } = default!;
    public string EntityId { get; init; } = default!;
    public AuditAction Action { get; init; }
    public DateTimeOffset TimestampUtc { get; init; }
    public string PayloadJson { get; init; } = default!;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}