using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure;
using ShopSphere.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.MapDefaultEndpoints();

// Migrate + seed BEFORE serving traffic.
await DatabaseStartup.MigrateAndSeedAsync(app.Services);

app.MapGet("/", () => "ShopSphere API — Day 12 alive!");

app.MapGet("/_debug/categories", async (ShopSphereDbContext db) =>
    await db.Categories
        .AsNoTracking()
        .Select(c => new { c.Id, c.Name, Slug = c.Slug.Value })
        .ToListAsync());

app.MapGet("/_debug/products", async (ShopSphereDbContext db) =>
    await db.Products
        .AsNoTracking()
        .Select(p => new
        {
            p.Id,
            p.Title,
            Slug = p.Slug.Value,
            Sku = p.Sku.Value,
            Price = p.Price.Amount + " " + p.Price.Currency,
            Status = p.Status.ToString()
        })
        .ToListAsync());

app.Run();