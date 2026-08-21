using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopSphere.Infrastructure.Persistence;

public sealed class ShopSphereDbContextFactory
    : IDesignTimeDbContextFactory<ShopSphereDbContext>
{
    public ShopSphereDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__shopsphere")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__shopsphere environment variable was not found.");

        var optionsBuilder =
            new DbContextOptionsBuilder<ShopSphereDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ShopSphereDbContext(optionsBuilder.Options);
    }
}