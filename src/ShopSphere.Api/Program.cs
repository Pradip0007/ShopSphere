var builder = WebApplication.CreateBuilder(args);

// This one line wires OTel, health checks, service discovery, resilience.
builder.AddServiceDefaults();

var app = builder.Build();

// Maps /health/live and /health/ready — free.
app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 2 alive!");

app.Run();