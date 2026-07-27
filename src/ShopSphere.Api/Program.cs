using ShopSphere.Domain.Catalog;

var builder = WebApplication.CreateBuilder(args);

// This one line wires OTel, health checks, service discovery, resilience.
builder.AddServiceDefaults();

var app = builder.Build();

// Maps /health/live and /health/ready — free.
app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 3 alive!");

// Sanity endpoint — proves Domain is wired in. We'll remove it once real endpoints exist.
app.MapGet("/_debug/new-product-id", () => new { id = ProductId.New().ToString() });

app.Run();