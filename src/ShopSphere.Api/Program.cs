using System.Reflection;
using FluentValidation;
using MediatR;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Scalar.AspNetCore;
using ShopSphere.Api.Behaviors;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Api.Middleware;
using ShopSphere.Infrastructure;
using ShopSphere.Api.Auth;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(o => o.Key.Length >= 32, "Jwt:Key must be at least 32 characters.")
    .ValidateOnStart();

// Replace the placeholder registered on Day 26.
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton(TimeProvider.System);

JwtOptions jwt = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt options missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

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
            Description = "Enterprise e-commerce platform.",
        };

        return Task.CompletedTask;
    });
});

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "ShopSphere API";
        options.Theme = ScalarTheme.Purple;
    });
}

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints(versionedGroup);

app.Run();