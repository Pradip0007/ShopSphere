var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ShopSphere_Api>("api")
    .WithHttpHealthCheck("/health");

builder.Build().Run();