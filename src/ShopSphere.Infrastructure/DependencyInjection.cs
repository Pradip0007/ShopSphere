using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopSphere.Domain.Common;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionStringName = "shopsphere")
    {
        services.AddDbContext<ShopSphereDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var cs = config.GetConnectionString(connectionStringName)
                ?? config[$"ConnectionStrings:{connectionStringName}"]
                ?? Environment.GetEnvironmentVariable($"ConnectionStrings__{connectionStringName}");

            if (string.IsNullOrWhiteSpace(cs))
            {
                cs = "Server=127.0.0.1,1433;Database=shopsphere;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=False;";
            }

            options.UseSqlServer(cs);
        });

        // Day 21 replaces this with the MediatR-backed dispatcher.
        services.AddSingleton<IDomainEventDispatcher, NullDomainEventDispatcher>();

        // Readiness check — SQL reachable + schema present.
        services
            .AddHealthChecks()
            .AddDbContextCheck<ShopSphereDbContext>(
                name: "sql",
                tags: ["ready"]);

        return services;
    }
}