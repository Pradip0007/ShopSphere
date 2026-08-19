namespace ShopSphere.Api.Consumers;

public sealed record PingCommand(Guid Id, DateTimeOffset EmittedAtUtc, string Note);