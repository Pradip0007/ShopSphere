using Microsoft.EntityFrameworkCore;
using Serilog;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure;
using ShopSphere.Infrastructure.Persistence;

// Bootstrap logger — used only during startup, then replaced.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.Host.UseSerilog(
    (ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "ShopSphere.Api")
        .WriteTo.Console(
            outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}> {Properties:j}{NewLine}{Exception}"),
    writeToProviders: true);

    builder.Services.AddInfrastructure();

    var app = builder.Build();

    app.UseSerilogRequestLogging(); // one clean line per HTTP request

    app.MapDefaultEndpoints();

    await DatabaseStartup.MigrateAndSeedAsync(app.Services);

    app.MapGet("/", () => "ShopSphere API — Day 14 alive!");

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

    // Demonstrates structured logging — note the {Sku} placeholder.
    app.MapGet("/_debug/log", (ILogger<Program> logger, string? sku) =>
    {
        logger.LogInformation("Debug log ping for {Sku}", sku ?? "UNKNOWN");
        return Results.Ok(new { logged = true });
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ShopSphere.Api terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}