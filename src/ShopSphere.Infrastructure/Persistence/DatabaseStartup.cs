using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopSphere.Infrastructure.Persistence.Seeding;

namespace ShopSphere.Infrastructure.Persistence;

public static class DatabaseStartup
{
    /// <summary>
    /// Applies pending EF migrations, then runs the dev seeder if in Development.
    /// Safe on cold or warm databases.
    /// </summary>
    public static async Task MigrateAndSeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopSphereDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseStartup));

        logger.LogInformation("Applying database migrations…");
        await db.Database.MigrateAsync(ct);

        if (env.IsDevelopment())
        {
            await DevDataSeeder.SeedAsync(db, logger, ct);
        }
    }
}