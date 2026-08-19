var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithDataVolume()
    .WithRedisCommander();

var rabbit = builder.AddRabbitMQ("rabbit")
    .WithDataVolume()
    .WithManagementPlugin(port: 15672);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("shopsphere");

var api = builder.AddProject<Projects.ShopSphere_Api>("api")
    .WithReference(cache)
    .WithReference(sql)
    .WithReference(rabbit)
    .WaitFor(sql);

builder.Build().Run();