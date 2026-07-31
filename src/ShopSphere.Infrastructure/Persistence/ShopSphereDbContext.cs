using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Inventory;

namespace ShopSphere.Infrastructure.Persistence;

public sealed class ShopSphereDbContext(DbContextOptions<ShopSphereDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopSphereDbContext).Assembly);
    }
}