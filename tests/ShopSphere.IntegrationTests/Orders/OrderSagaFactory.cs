using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopSphere.Api.Features.Inventory;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory;
using ShopSphere.Domain.Payments;
using ShopSphere.Infrastructure.Outbox;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Orders;

public sealed class OrderSagaFactory : IntegrationTestFactory
{
    public FakePaymentGateway Payment { get; }

    public OrderSagaFactory(ContainerFixture containers, bool paymentSucceeds)
        : base(containers)
    {
        Payment = new FakePaymentGateway(paymentSucceeds);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPaymentGateway>();
            services.AddSingleton<IPaymentGateway>(Payment);
            services.RemoveAll<IInventoryClient>();
            services.AddScoped<IInventoryClient, SqlInventoryClient>();
            services.AddHostedService<TestOutboxDispatcher>();
        });
    }
}

public sealed class FakePaymentGateway(bool succeeds) : IPaymentGateway
{
    public int AuthorizeCallCount { get; private set; }
    public List<string> IdempotencyKeys { get; } = [];

    public Task<AuthorizationResult> AuthorizeAsync(
        Money amount,
        string paymentMethodId,
        string idempotencyKey,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken ct = default)
    {
        AuthorizeCallCount++;
        IdempotencyKeys.Add(idempotencyKey);
        return Task.FromResult(
            succeeds
                ? new AuthorizationResult(true, $"pi-test-{AuthorizeCallCount}", null)
                : new AuthorizationResult(false, null, "card_declined"));
    }
}

internal sealed class SqlInventoryClient(ShopSphereDbContext db) : IInventoryClient
{
    public async Task<ReserveResult> ReserveAsync(
        Guid orderId,
        string sku,
        int quantity,
        CancellationToken ct)
    {
        var stock = (await db.StockLevels.ToListAsync(ct))
            .SingleOrDefault(level => level.Sku.Value == sku);

        if (stock is null)
            return new ReserveResult(false, 0, ReserveFailReason.UnknownSku, "SKU not found.");

        var result = stock.Reserve(quantity);
        if (result.IsFailure)
        {
            return new ReserveResult(
                false,
                stock.Available,
                ReserveFailReason.InsufficientStock,
                result.Error.Message);
        }

        await db.SaveChangesAsync(ct);
        return new ReserveResult(true, stock.Available, null, null);
    }

    public async Task<ReleaseResult> ReleaseAsync(
        Guid orderId,
        string sku,
        int quantity,
        CancellationToken ct)
    {
        var stock = (await db.StockLevels.ToListAsync(ct))
            .Single(level => level.Sku.Value == sku);
        var result = stock.Release(quantity);
        if (result.IsFailure)
            return new ReleaseResult(false, stock.Available);

        await db.SaveChangesAsync(ct);
        return new ReleaseResult(true, stock.Available);
    }
}

internal sealed class TestOutboxDispatcher(
    IServiceScopeFactory scopes,
    MassTransit.IBus bus,
    ILogger<TestOutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Test outbox dispatch failed.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
        }
    }

    private async Task DispatchPendingAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopSphereDbContext>();
        var pending = await db.OutboxMessages
            .Where(message => message.ProcessedAtUtc == null)
            .OrderBy(message => message.OccurredAtUtc)
            .Take(20)
            .ToListAsync(ct);

        foreach (var message in pending)
        {
            var type = Type.GetType(message.Type, throwOnError: true)!;
            var payload = JsonSerializer.Deserialize(message.PayloadJson, type)!;
            await bus.Publish(payload, type, ct);
            message.ProcessedAtUtc = DateTimeOffset.UtcNow;
        }

        if (pending.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
