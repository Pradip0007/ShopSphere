var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithDataVolume()
    .WithRedisCommander();

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("shopsphere");

var api = builder.AddProject<Projects.ShopSphere_Api>("api")
    .WithReference(cache)
    .WithReference(sql)
    .WaitFor(sql);

builder.Build().Run();