using System.Reflection;
using ShopSphere.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// NEW: scans this assembly for every IEndpoint and registers them.
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 16 alive!");

// NEW: invokes MapEndpoint on every registered IEndpoint.
app.MapEndpoints();

app.Run();