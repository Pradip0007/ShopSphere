using ShopSphere.Inventory.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcService<InventoryService>();

app.MapGet("/", () =>
    "ShopSphere.Inventory.Grpc — gRPC service is running. Use a gRPC client.");

app.Run();