var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithDataVolume()
    .WithRedisCommander();

var rabbit = builder.AddRabbitMQ("rabbit")
    .WithDataVolume()
    .WithManagementPlugin(port: 15672);

var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog", "v1.0.1")
    .WithHttpEndpoint(port: 8025, targetPort: 8025, name: "ui")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp");

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("shopsphere");

var api = builder.AddProject<Projects.ShopSphere_Api>("api")
    .WithReference(cache)
    .WithReference(sql)
    .WithReference(rabbit)
    .WithEnvironment("Email__Host", "localhost")
    .WithEnvironment("Email__Port", "1025")
    .WaitFor(sql);

builder.Build().Run();