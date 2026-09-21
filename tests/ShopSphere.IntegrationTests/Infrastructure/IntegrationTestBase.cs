using Microsoft.Extensions.DependencyInjection;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase(
    ContainerFixture containers,
    bool enableRateLimiter = false)
    : IAsyncLifetime
{
    protected IntegrationTestFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    protected HttpClient CreateClient() => Factory.CreateClient();

    protected async Task UsingScope(Func<ShopSphereDbContext, Task> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopSphereDbContext>();
        await action(db);
    }

    protected async Task<T> UsingScope<T>(Func<ShopSphereDbContext, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopSphereDbContext>();
        return await action(db);
    }

    public async Task InitializeAsync()
    {
        Factory = new IntegrationTestFactory(containers, enableRateLimiter);
        Client = Factory.CreateClient();
        await Factory.ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }
}
