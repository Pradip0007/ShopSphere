using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShopSphere.Api.HealthChecks;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure;
using ShopSphere.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "ShopSphere.Api")
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
            "<{SourceContext}> {Properties:j}{NewLine}{Exception}"));

    builder.Services.AddInfrastructure();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.MapDefaultEndpoints();

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = JsonHealthCheckWriter.WriteAsync
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("ready"),
        ResponseWriter = JsonHealthCheckWriter.WriteAsync
    });

    await DatabaseStartup.MigrateAndSeedAsync(app.Services);

    app.MapGet("/", () => "ShopSphere API — Day 15 alive!");

    app.MapGet("/_debug/categories", async (ShopSphereDbContext db) =>
        await db.Categories.AsNoTracking()
            .Select(c => new { c.Id, c.Name, Slug = c.Slug.Value })
            .ToListAsync());

    app.MapGet("/_debug/products", async (ShopSphereDbContext db) =>
        await db.Products.AsNoTracking()
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
}
catch (Exception ex)
{
    Log.Fatal(ex, "ShopSphere.Api terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}