using Microsoft.EntityFrameworkCore;
using ShopSphere.Infrastructure.Audit;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Admin;

public static class AuditQuery
{
    public static async Task<IResult> HandleAsync(
        ShopSphereDbContext db,
        string? entityType,
        string? entityId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int take = 50,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);

        IQueryable<AuditLog> q = db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityType))
            q = q.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(entityId))
            q = q.Where(a => a.EntityId == entityId);

        if (fromUtc.HasValue)
            q = q.Where(a => a.TimestampUtc >= fromUtc);

        if (toUtc.HasValue)
            q = q.Where(a => a.TimestampUtc <= toUtc);

        var rows = await q
            .OrderByDescending(a => a.TimestampUtc)
            .Take(take)
            .ToListAsync(ct);

        return Results.Ok(rows.Select(r => new
        {
            r.Id,
            r.EntityType,
            r.EntityId,
            action = r.Action.ToString(),
            r.TimestampUtc,
            r.ActorUserId,
            r.IpAddress
        }));
    }
}