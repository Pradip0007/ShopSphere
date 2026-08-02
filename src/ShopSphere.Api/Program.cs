using System.Reflection;
using FluentValidation;
using MediatR;
using ShopSphere.Api.Behaviors;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure();   

Assembly apiAssembly = Assembly.GetExecutingAssembly();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(apiAssembly);

    // Outer first. Log wraps Validate wraps Handler.
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Scans and registers every IValidator<T> as scoped.
builder.Services.AddValidatorsFromAssembly(apiAssembly);

builder.Services.AddEndpoints(apiAssembly);
var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 18 alive!");

app.MapEndpoints();

app.Run();