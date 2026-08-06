using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Users;

public sealed record UserRegisteredEvent(
    UserId UserId,
    string Email,
    DateTimeOffset RegisteredAt)
    : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}