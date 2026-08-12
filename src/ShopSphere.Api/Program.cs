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
using ShopSphere.Api.Infrastructure;
using ShopSphere.Api.Infrastructure.Cart;
using ShopSphere.Api.Infrastructure.Redis;
using ShopSphere.Api.Middleware;

using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Users;

using ShopSphere.Infrastructure;

using IDatabase = StackExchange.Redis.IDatabase;


var builder = WebApplication.CreateBuilder(args);


// ------------------------------------------------------------
// Aspire service defaults
// ------------------------------------------------------------

builder.AddServiceDefaults();


// ------------------------------------------------------------
// Redis
// ------------------------------------------------------------

builder.Services.AddShopSphereRedis(builder.Configuration);

builder.Services.AddShopSphereCart();


// ------------------------------------------------------------
// Health checks
// ------------------------------------------------------------

builder.Services
    .AddHealthChecks()
    .AddRedis(
        redisConnectionString:
            builder.Configuration.GetConnectionString("cache")!,
        name: "redis",
        tags: ["ready"]);


// ------------------------------------------------------------
// Infrastructure / EF Core
// ------------------------------------------------------------

builder.Services.AddInfrastructure();


// ------------------------------------------------------------
// MediatR
// ------------------------------------------------------------

Assembly apiAssembly = Assembly.GetExecutingAssembly();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(apiAssembly);

    cfg.AddOpenBehavior(
        typeof(LoggingBehavior<,>));

    cfg.AddOpenBehavior(
        typeof(ValidationBehavior<,>));
});


// ------------------------------------------------------------
// FluentValidation
// ------------------------------------------------------------

builder.Services.AddValidatorsFromAssembly(apiAssembly);


// ------------------------------------------------------------
// Endpoint discovery
// ------------------------------------------------------------

builder.Services.AddEndpoints(apiAssembly);


// ------------------------------------------------------------
// JWT configuration
// ------------------------------------------------------------

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        o => o.Key.Length >= 32,
        "Jwt:Key must be at least 32 characters.")
    .ValidateOnStart();


// ------------------------------------------------------------
// JWT services
// ------------------------------------------------------------

builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services.AddSingleton(TimeProvider.System);


// ------------------------------------------------------------
// Authorization policies
// ------------------------------------------------------------

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


// ------------------------------------------------------------
// Read JWT options
// ------------------------------------------------------------

JwtOptions jwt =
    builder.Configuration
        .GetSection(JwtOptions.SectionName)
        .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "Jwt options missing.");


// ------------------------------------------------------------
// Authentication
// ------------------------------------------------------------

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

                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
    });


// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------

builder.Services.AddAuthorization();


// ------------------------------------------------------------
// Exception handling
// ------------------------------------------------------------

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] =
            ctx.HttpContext.TraceIdentifier;
    };
});


// ------------------------------------------------------------
// OpenAPI
// ------------------------------------------------------------

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
                    Description =
                        "Enterprise e-commerce platform."
                };

                return Task.CompletedTask;
            });
    });


// ------------------------------------------------------------
// API Versioning
// ------------------------------------------------------------

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


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ------------------------------------------------------------
// Exception handling middleware
// ------------------------------------------------------------

app.UseExceptionHandler();


// ------------------------------------------------------------
// Aspire default endpoints
// ------------------------------------------------------------

app.MapDefaultEndpoints();


// ------------------------------------------------------------
// Root endpoint
// ------------------------------------------------------------

app.MapGet(
    "/",
    () => Results.Redirect("/scalar/v1"));


// ============================================================
// DEVELOPMENT-ONLY DEBUG ENDPOINTS
// ============================================================

if (app.Environment.IsDevelopment())
{
    // --------------------------------------------------------
    // Redis ping
    // --------------------------------------------------------

    app.MapGet(
        "/debug/redis-ping",
        async (IDatabase db) =>
        {
            var latency =
                await db.PingAsync();

            return Results.Ok(new
            {
                pong = true,

                latencyMs =
                    latency.TotalMilliseconds,

                endpoint =
                    db.Multiplexer
                        .GetEndPoints()[0]
                        .ToString()
            });
        });


    // --------------------------------------------------------
    // Cart smoke test
    // --------------------------------------------------------

    app.MapPost(
        "/debug/cart-smoke",
        async (ICartRepository cart) =>
        {
            // Create a temporary guest/session cart.
            var key =
                CartKey.Session(Guid.NewGuid());


            // Create two fake products.
            // We are not checking SQL here.
            // These IDs are only being used to test Redis.
            var productA =
                ProductId.New();

            var productB =
                ProductId.New();


            // ------------------------------------------------
            // ADD PRODUCT A
            // ------------------------------------------------

            await cart.AddItemAsync(
                key,
                productA,
                2);

            // Product A = 2


            // ------------------------------------------------
            // ADD MORE PRODUCT A
            // ------------------------------------------------

            await cart.AddItemAsync(
                key,
                productA,
                1);

            // Product A = 3
            // because AddItem is additive:
            // 2 + 1 = 3


            // ------------------------------------------------
            // ADD PRODUCT B
            // ------------------------------------------------

            await cart.AddItemAsync(
                key,
                productB,
                5);

            // Product B = 5


            // ------------------------------------------------
            // UPDATE PRODUCT B
            // ------------------------------------------------

            await cart.UpdateItemAsync(
                key,
                productB,
                4);

            // Product B = 4
            //
            // UpdateItem is absolute:
            // 5 -> 4
            //
            // It does NOT do:
            // 5 + 4 = 9


            // ------------------------------------------------
            // READ CART
            // ------------------------------------------------

            var loaded =
                await cart.GetAsync(key);


            // ------------------------------------------------
            // RETURN RESULT
            // ------------------------------------------------

            return Results.Ok(new
            {
                key =
                    key.ToRedisKey(),

                totalUnits =
                    loaded.TotalUnits,

                lines =
                    loaded.Lines.Select(
                        l => new
                        {
                            productId =
                                l.ProductId.ToString(),

                            qty =
                                l.Quantity
                        })
            });
        });


    // --------------------------------------------------------
    // OpenAPI + Scalar
    // --------------------------------------------------------

    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title =
            "ShopSphere API";

        options.Theme =
            ScalarTheme.Purple;
    });
}


// ============================================================
// API VERSIONED ROUTES
// ============================================================

ApiVersionSet apiVersionSet =
    app.NewApiVersionSet()

        .HasApiVersion(
            new ApiVersion(1))

        .ReportApiVersions()

        .Build();


RouteGroupBuilder versionedGroup =
    app
        .MapGroup(
            "/api/v{version:apiVersion}")

        .WithApiVersionSet(
            apiVersionSet);


// ------------------------------------------------------------
// Authentication / Authorization middleware
// ------------------------------------------------------------

app.UseAuthentication();

app.UseAuthorization();


// ------------------------------------------------------------
// ShopSphere endpoints
// ------------------------------------------------------------

app.MapEndpoints(
    versionedGroup);


// ------------------------------------------------------------
// Start application
// ------------------------------------------------------------

app.Run();