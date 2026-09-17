using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Respawn;

using ShopSphere.Infrastructure.Persistence;
using ShopSphere.IntegrationTests.Common;

namespace ShopSphere.IntegrationTests.Infrastructure;

public class IntegrationTestFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly ContainerFixture _containers;
    private Respawner? _respawner;

    public IntegrationTestFactory(ContainerFixture containers)
        => _containers = containers;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

builder.UseSetting(
    "ConnectionStrings:shopsphere",
    _containers.SqlConnectionString);

builder.UseSetting(
    "ConnectionStrings:cache",
    _containers.RedisConnectionString);

builder.UseSetting(
    "ConnectionStrings:rabbit",
    _containers.RabbitMqConnectionString);

builder.UseSetting(
    "Jwt:Issuer",
    "ShopSphere.IntegrationTests");

builder.UseSetting(
    "Jwt:Audience",
    "ShopSphere.IntegrationTests");

builder.UseSetting(
    "Jwt:Key",
    "ShopSphere.IntegrationTests.Test.Jwt.Key.2026.32Chars!");

builder.UseSetting(
    "Stripe:SecretKey",
    "sk_test_shopsphere_integration");

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        _respawner ??= await Respawner.CreateAsync(
            _containers.SqlConnectionString,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore = ["__EFMigrationsHistory"]
            });

        await _respawner.ResetAsync(_containers.SqlConnectionString);
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ShopSphereDbContext>();

        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
