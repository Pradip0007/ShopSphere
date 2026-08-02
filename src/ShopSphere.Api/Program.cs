using System.Reflection;
using FluentValidation;
using MediatR;
using Scalar.AspNetCore;
using ShopSphere.Api.Behaviors;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Api.Middleware;
using ShopSphere.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure();   

Assembly apiAssembly = Assembly.GetExecutingAssembly();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(apiAssembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(apiAssembly);
builder.Services.AddEndpoints(apiAssembly);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new()
        {
            Title = "ShopSphere API",
            Version = "v1",
            Description = "Enterprise e-commerce platform — REST endpoints for catalog, orders, checkout.",
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();          // exposes GET /openapi/v1.json
    app.MapScalarApiReference(options =>
    {
        options.Title = "ShopSphere API";
        options.Theme = ScalarTheme.Purple;
    });
    // Now available at /scalar/v1
}

app.MapGet("/", () => "ShopSphere API — Day 20 alive!");

app.MapEndpoints();

app.Run();