using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShopSphere.Domain.Common;

namespace ShopSphere.Api.Infrastructure.Outbox;

public sealed class OutboxSaveInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventToIntegrationMapper _mapper;
    private readonly ILogger<OutboxSaveInterceptor> _logger;

    public OutboxSaveInterceptor(
        IDomainEventToIntegrationMapper mapper,
        ILogger<OutboxSaveInterceptor> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var aggregates = ctx.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var integration = _mapper.Map(domainEvent);
                if (integration is null)
                {
                    _logger.LogDebug(
                        "No integration mapping for {DomainEvent} — skipping.",
                        domainEvent.GetType().FullName);
                    continue;
                }

                var payload = JsonSerializer.Serialize(integration, integration.GetType());
                ctx.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = integration.GetType().AssemblyQualifiedName ?? integration.GetType().FullName!,
                    PayloadJson = payload,
                    OccurredAtUtc = DateTimeOffset.UtcNow
                });
            }

            aggregate.ClearDomainEvents();
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
