using System.Reflection;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure();   // <-- ADD THIS

Assembly apiAssembly = Assembly.GetExecutingAssembly();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(apiAssembly));

builder.Services.AddEndpoints(apiAssembly);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 17 alive!");

app.MapEndpoints();

app.Run();