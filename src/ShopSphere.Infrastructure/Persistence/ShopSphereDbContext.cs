using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Inventory;
using ShopSphere.Domain.Users;
using ShopSphere.Domain.Ordering;
using ShopSphere.Domain.Reviews;
using ShopSphere.Infrastructure.Audit;
using ShopSphere.Infrastructure.Outbox;

namespace ShopSphere.Infrastructure.Persistence;

public sealed class ShopSphereDbContext(DbContextOptions<ShopSphereDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopSphereDbContext).Assembly);
    }
}