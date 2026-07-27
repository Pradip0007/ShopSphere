using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Catalog.Events;

/// <summary>
/// Temporary — used only by the /_debug/raise endpoint. Delete after Day 6.
/// </summary>
public sealed record PingEvent(string Message) : DomainEvent;