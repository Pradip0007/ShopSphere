using ShopSphere.IntegrationTests.Common;

namespace ShopSphere.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase(ContainerFixture containers)
    : IAsyncLifetime
{
    protected IntegrationTestFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new IntegrationTestFactory(containers);
        Client = Factory.CreateClient();
        await Factory.ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }
}
