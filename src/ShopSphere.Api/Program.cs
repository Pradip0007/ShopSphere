using System.Reflection;
using System.Text;

using Asp.Versioning;
using Asp.Versioning.Builder;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Scalar.AspNetCore;

using ShopSphere.Api.Auth;
using ShopSphere.Api.Behaviors;
using ShopSphere.Api.Features.Auth;
using ShopSphere.Api.Features.Cart;
using ShopSphere.Api.Features.Checkout;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Api.Infrastructure.Cart;
using ShopSphere.Api.Infrastructure.Redis;
using ShopSphere.Api.Middleware;

using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Ordering;
using ShopSphere.Domain.Users;
using ShopSphere.Api.Infrastructure.Payments;
using ShopSphere.Domain.Payments;
using ShopSphere.Infrastructure;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.Api.Infrastructure.Messaging;
using MassTransit;
using ShopSphere.Api.Consumers;
using ShopSphere.Api.Features.Webhooks.Stripe;
using IDatabase = StackExchange.Redis.IDatabase;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddShopSphereRedis(builder.Configuration);
builder.Services.AddShopSphereCart();
builder.Services.AddShopSphereMessaging(builder.Configuration);

builder.Services
    .AddHealthChecks()
    .AddRedis(
        redisConnectionString:
            builder.Configuration.GetConnectionString("cache")!,
        name: "redis",
        tags: ["ready"]);

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

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        o => o.Key.Length >= 32,
        "Jwt:Key must be at least 32 characters.")
    .ValidateOnStart();

builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton<IProcessedMessageStore,InMemoryProcessedMessageStore>();

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(
        Permissions.ProductsWrite,
        p => p.RequireClaim(
            "permission",
            Permissions.ProductsWrite))
    .AddPolicy(
        Permissions.ProductsRead,
        p => p.RequireClaim(
            "permission",
            Permissions.ProductsRead))
    .AddPolicy(
        Permissions.OrdersReadSelf,
        p => p.RequireClaim(
            "permission",
            Permissions.OrdersReadSelf))
    .AddPolicy(
        Permissions.OrdersReadAll,
        p => p.RequireClaim(
            "permission",
            Permissions.OrdersReadAll))
    .AddPolicy(
        Permissions.OrdersManage,
        p => p.RequireClaim(
            "permission",
            Permissions.OrdersManage));

JwtOptions jwt =
    builder.Configuration
        .GetSection(JwtOptions.SectionName)
        .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "Jwt options missing.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata =
            !builder.Environment.IsDevelopment();

        options.SaveToken = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,

                ValidateAudience = true,
                ValidAudience = jwt.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Key)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] =
            ctx.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.AddDocumentTransformer(
            (document, _, _) =>
            {
                document.Info = new()
                {
                    Title = "ShopSphere API",
                    Version = "v1",
                    Description = "Enterprise e-commerce platform."
                };

                return Task.CompletedTask;
            });
    });

builder.Services.AddScoped<IOrderRepository, ShopSphere.Infrastructure.Persistence.SqlOrderRepository>();
builder.Services.AddSingleton<IProcessedWebhookStore, InMemoryProcessedWebhookStore>();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddOptions<StripeOptions>()
    .Bind(builder.Configuration.GetSection(StripeOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IPaymentGateway, StripePaymentGateway>();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(1);

        options.AssumeDefaultVersionWhenUnspecified =
            true;

        options.ReportApiVersions = true;

        options.ApiVersionReader =
            new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapGet(
    "/",
    () => Results.Redirect("/scalar/v1"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "ShopSphere API";
        options.Theme = ScalarTheme.Purple;
    });
}

ApiVersionSet apiVersionSet =
    app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();

RouteGroupBuilder versionedGroup =
    app
        .MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

///commad for dataseed
await DatabaseStartup.MigrateAndSeedAsync(app.Services);

app.MapStripeWebhook();

app.UseAuthentication();

app.UseAuthorization();

app.MapCartEndpoints();

app.MapCheckoutEndpoints();

app.MapEndpoints(versionedGroup);

app.Run();