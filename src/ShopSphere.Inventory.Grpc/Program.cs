using ShopSphere.Inventory.Grpc.Services;
using ShopSphere.Inventory.Grpc.Streaming;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Infrastructure.Persistence;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<ShopSphereDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("shopsphere")));

builder.Services.AddGrpcHealthChecks()
    .AddDbContextCheck<ShopSphereDbContext>("sql");

builder.Services.AddMassTransit(mt =>
{
    mt.SetKebabCaseEndpointNameFormatter();

    mt.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            builder.Configuration.GetConnectionString("rabbit"));

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddGrpc(o =>
{
    o.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddSingleton<ILowStockChannel, LowStockChannel>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcHealthChecksService();

app.MapGrpcService<InventoryService>();

app.MapGet("/", () =>
    "ShopSphere.Inventory.Grpc — gRPC service is running. Use a gRPC client.");

app.Run();