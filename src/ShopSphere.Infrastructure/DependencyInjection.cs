using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopSphere.Domain.Common;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Security;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionStringName = "shopsphere")
    {
        services.AddDbContext<ShopSphereDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var cs = config.GetConnectionString(connectionStringName)
                ?? config[$"ConnectionStrings:{connectionStringName}"]
                ?? Environment.GetEnvironmentVariable($"ConnectionStrings__{connectionStringName}");

            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException(
                    $"Connection string '{connectionStringName}' was not found.");
            }

            options.UseSqlServer(cs);
        });

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

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