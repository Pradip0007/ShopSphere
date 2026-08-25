using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using ShopSphere.Domain.Common;

namespace ShopSphere.Infrastructure.Outbox;

public sealed class OutboxSaveInterceptor(
    IIntegrationEventMapperResolver resolver,
    ILogger<OutboxSaveInterceptor> logger) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;

        if (ctx is null)
        {
            return base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        var aggregates = ctx.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var integration = resolver.Map(domainEvent);

                if (integration is null)
                {
                    logger.LogDebug(
                        "No mapping for {DomainEvent}",
                        domainEvent.GetType().FullName);

                    continue;
                }

                var payload = JsonSerializer.Serialize(
                    integration,
                    integration.GetType());

                ctx.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),

                    Type = integration.GetType()
                        .AssemblyQualifiedName
                        ?? integration.GetType().FullName!,

                    PayloadJson = payload,

                    OccurredAtUtc = DateTimeOffset.UtcNow
                });
            }

            aggregate.ClearDomainEvents();
        }

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
}