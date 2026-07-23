using AiStudyOS.Application.Analytics.Ai;
using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Quiz.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Analytics.Queries.GetDashboardAnalytics;

/// <summary>
/// Short-TTL IMemoryCache'd (60s) — this is the one Analytics query hit on every single Dashboard
/// page load, so unlike the deeper /analytics endpoints it's worth the staleness tradeoff of a
/// brief cache window. Mirrors CachedAiKernel's identical IMemoryCache-for-a-hot-path pattern.
/// </summary>
public class GetDashboardAnalyticsQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator,
    IMemoryCache cache,
    IOptions<AnalyticsOptions> options,
    ILogger<GetDashboardAnalyticsQueryHandler> logger) : IQueryHandler<GetDashboardAnalyticsQuery, DashboardAnalyticsDto>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public async ValueTask<DashboardAnalyticsDto> Handle(GetDashboardAnalyticsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var cacheKey = $"dashboard-analytics:{userId}";

        if (cache.TryGetValue(cacheKey, out DashboardAnalyticsDto? cached) && cached is not null)
            return cached;

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var weekStart = today.AddDays(-6);

        var mastery = await db.TopicMasteries
            .Where(m => m.UserId == userId && m.MasteryScore < 0.6)
            .OrderBy(m => m.MasteryScore)
            .Take(options.Value.WeakStrongTopicsTake)
            .ToListAsync(ct);
        var weakTopics = mastery.Select(TopicMasteryDto.FromDomain).ToList();

        var weeklyActivity = await AnalyticsQueryHelpers.ComputeDailyActivityAsync(db, userId, weekStart, today, ct);
        var goals = await AnalyticsQueryHelpers.ComputeGoalAnalyticsAsync(db, userId, ct);
        var insights = await InsightsQueryHelper.GetOrGenerateAsync(db, mediator, dateTimeProvider, logger, userId, ct);

        var result = new DashboardAnalyticsDto(weakTopics, weeklyActivity, goals, insights);
        cache.Set(cacheKey, result, CacheDuration);
        return result;
    }
}
