using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ShopSphere.Infrastructure.Audit;

public sealed class AuditInterceptor(
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuditInterceptor> logger) : SaveChangesInterceptor
{
    private static readonly HashSet<string> ExcludedEntityTypes = new(StringComparer.Ordinal)
    {
        // Don't audit the audit log itself — infinite loop.
        nameof(AuditLog),
        // Don't audit outbox rows — the mapped integration event is the semantic record.
        "OutboxMessage"
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var actor = ExtractActor();
        var (ip, ua) = ExtractRequestMeta();

        foreach (var entry in ctx.ChangeTracker.Entries<AuditLog>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new InvalidOperationException("AuditLog is append-only.");
            }
        }

        var entries = ctx.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => !ExcludedEntityTypes.Contains(e.Metadata.Name.Split('.').Last()))
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => AuditAction.Update
            };

            var payload = SerializeEntry(entry, action);
            var entityId = ResolveEntityId(entry);

            ctx.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = actor,
                EntityType = entry.Metadata.Name,
                EntityId = entityId,
                Action = action,
                TimestampUtc = DateTimeOffset.UtcNow,
                PayloadJson = payload,
                IpAddress = ip,
                UserAgent = ua
            });
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private Guid? ExtractActor()
    {
        var http = httpContextAccessor.HttpContext;
        if (http?.User is null) return null;
        var raw = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? http.User.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var g) ? g : null;
    }

    private (string? Ip, string? Ua) ExtractRequestMeta()
    {
        var http = httpContextAccessor.HttpContext;
        if (http is null) return (null, null);
        var ip = http.Connection.RemoteIpAddress?.ToString();
        var ua = http.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(ua)) ua = null;
        return (ip, ua);
    }

    private static string ResolveEntityId(EntityEntry entry)
    {
        var pk = entry.Metadata.FindPrimaryKey();
        if (pk is null) return string.Empty;
        var parts = pk.Properties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "")
            .ToArray();
        return string.Join(":", parts);
    }

    private static string SerializeEntry(EntityEntry entry, AuditAction action)
    {
        var props = new Dictionary<string, object?>();
        foreach (var p in entry.Properties)
        {
            switch (action)
            {
                case AuditAction.Create:
                    props[p.Metadata.Name] = p.CurrentValue;
                    break;
                case AuditAction.Update:
                    if (p.IsModified)
                    {
                        props[p.Metadata.Name] = new { From = p.OriginalValue, To = p.CurrentValue };
                    }
                    break;
                case AuditAction.Delete:
                    props[p.Metadata.Name] = p.OriginalValue;
                    break;
            }
        }
        return JsonSerializer.Serialize(props);
    }
}