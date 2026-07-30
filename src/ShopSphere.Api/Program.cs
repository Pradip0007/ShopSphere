using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory;
using ShopSphere.Infrastructure;
using ShopSphere.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 10 alive!");

// Proves the DbContext resolves from DI and can talk to SQL.
app.MapGet("/_debug/db-ping", async (ShopSphereDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return new { canConnect };
});

app.Run();