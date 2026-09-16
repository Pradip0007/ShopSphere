using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace ShopSphere.IntegrationTests.Common;

public sealed class ContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7.4-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management-alpine")
        .Build();

    public string SqlConnectionString => _sqlServer.GetConnectionString();

    public string RedisConnectionString => _redis.GetConnectionString();

    public string RabbitMqConnectionString => _rabbitMq.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _sqlServer.StartAsync(),
            _redis.StartAsync(),
            _rabbitMq.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await _sqlServer.DisposeAsync();
        await _redis.DisposeAsync();
        await _rabbitMq.DisposeAsync();
    }
}
