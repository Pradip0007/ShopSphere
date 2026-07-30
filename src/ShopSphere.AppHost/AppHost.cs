var builder = DistributedApplication.CreateBuilder(args);

// Add a SQL Server container. `AddDatabase` gives Aspire a logical DB name
// so it can build a connection string like "Server=…;Database=shopsphere;…".
var sql = builder.AddSqlServer("sql")
                 .WithDataVolume()               // survives container restarts
                 .AddDatabase("shopsphere");

var api = builder.AddProject<Projects.ShopSphere_Api>("api")
                 .WithReference(sql);            // injects ConnectionStrings:shopsphere

builder.Build().Run();