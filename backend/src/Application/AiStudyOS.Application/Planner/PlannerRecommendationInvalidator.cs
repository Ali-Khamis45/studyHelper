using AiStudyOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Planner;

/// <summary>
/// Invalidates today's active recommendation whenever the data it was generated from changes, so
/// GetTodayQueryHandler's cache check (PlannerRecommendation.IsActive) naturally forces a
/// regeneration on the next read instead of serving a now-stale plan (§2).
/// </summary>
public static class PlannerRecommendationInvalidator
{
    public static async Task InvalidateTodayAsync(IApplicationDbContext db, Guid userId, DateTime nowUtc, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(nowUtc);

        var active = await db.PlannerRecommendations
            .Where(r => r.UserId == userId && r.Date == today && r.InvalidatedAt == null)
            .ToListAsync(ct);

        if (active.Count == 0) return;

        foreach (var recommendation in active)
            recommendation.Invalidate(nowUtc);

        await db.SaveChangesAsync(ct);
    }
}
