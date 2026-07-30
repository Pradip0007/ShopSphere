using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopSphere.Domain.Common;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the DbContext and any other infra services.
    /// Connection string is resolved via Aspire's service discovery — the
    /// AppHost injects it as configuration key "ConnectionStrings:shopsphere".
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionStringName = "shopsphere")
    {
        services.AddDbContext<ShopSphereDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var cs = config.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{connectionStringName}' not configured.");
            options.UseSqlServer(cs);
        });

        // Day 21 replaces this with the MediatR-backed dispatcher.
        services.AddSingleton<IDomainEventDispatcher, NullDomainEventDispatcher>();

        return services;
    }
}