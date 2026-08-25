using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Reviews;

public sealed record ReviewPostedEvent(
    ReviewId ReviewId,
    Guid UserId,
    ProductId ProductId,
    int Rating,
    DateTimeOffset OccurredAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt => OccurredAtUtc;
}

public sealed record ReviewApprovedEvent(
    ReviewId ReviewId,
    ProductId ProductId,
    Guid ModeratorUserId,
    DateTimeOffset OccurredAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt => OccurredAtUtc;
}

public sealed record ReviewRejectedEvent(
    ReviewId ReviewId,
    ProductId ProductId,
    Guid ModeratorUserId,
    string Reason,
    DateTimeOffset OccurredAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt => OccurredAtUtc;
}