namespace ShopSphere.Contracts.Events;

public sealed record WishlistItemAdded(
    Guid WishlistId,
    Guid UserId,
    Guid ProductId,
    DateTimeOffset OccurredAt);