namespace ShopSphere.Contracts.Events;

public sealed record AbandonedCartReminder(
    string CartKey,
    Guid? UserId,
    int LineCount,
    TimeSpan IdleFor,
    DateTimeOffset DetectedAtUtc);