using AiStudyOS.Application.Analytics.Commands.RegenerateInsights;
using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiStudyOS.Application.Analytics.Ai;

/// <summary>
/// Shared by every query that embeds Insights (Overview, Dashboard) — reuses a still-active cached
/// report if one exists, generates a fresh one otherwise, and degrades to null (never throws) on AI
/// failure so an Insights outage never blocks the rest of an Analytics/Dashboard response. Mirrors
/// GetTodayQueryHandler's identical cache-or-generate-or-degrade shape for PlannerRecommendation.
/// </summary>
public static class InsightsQueryHelper
{
    public static async Task<InsightsDto?> GetOrGenerateAsync(
        IApplicationDbContext db, IMediator mediator, IDateTimeProvider dateTimeProvider, ILogger logger, Guid userId, CancellationToken ct)
    {
        var now = dateTimeProvider.UtcNow;

        var existing = await db.AnalyticsInsights
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.GeneratedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (existing is not null && existing.IsActive(now))
            return InsightsFinalizer.ToDto(existing);

        try
        {
            return await mediator.Send(new RegenerateInsightsCommand(), ct);
        }
        catch (AiGenerationFailedException ex)
        {
            logger.LogWarning(ex, "Insights generation failed for user {UserId}; serving Analytics/Dashboard without them", userId);
            return null;
        }
    }
}
