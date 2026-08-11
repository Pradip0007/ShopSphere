namespace ShopSphere.Api.Infrastructure.Redis;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string KeyPrefix { get; init; } = "shopsphere";

    public TimeSpan DefaultTtl { get; init; } = TimeSpan.FromDays(30);
}