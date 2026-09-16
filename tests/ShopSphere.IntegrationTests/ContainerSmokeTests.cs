using Microsoft.Data.SqlClient;
using RabbitMQ.Client;
using ShopSphere.IntegrationTests.Common;

namespace ShopSphere.IntegrationTests;

[Collection("Containers")]
public sealed class ContainerSmokeTests(ContainerFixture fixture)
{
    [Fact]
    public async Task SqlServer_should_accept_connections()
    {
        await using var connection = new SqlConnection(fixture.SqlConnectionString);

        await connection.OpenAsync();

        await using var command = new SqlCommand("SELECT 1", connection);

        var result = await command.ExecuteScalarAsync();

        result.Should().Be(1);
    }

    [Fact]
    public async Task Redis_should_accept_connections()
    {
        var connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(
            fixture.RedisConnectionString);

        var database = connection.GetDatabase();
        var result = await database.PingAsync();

        result.Should().BeGreaterThan(TimeSpan.Zero);

        await connection.CloseAsync();
        connection.Dispose();
    }

    [Fact]
    public async Task RabbitMq_should_accept_connections()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(fixture.RabbitMqConnectionString)
        };

        using var connection = factory.CreateConnection();

        connection.IsOpen.Should().BeTrue();
    }

}
