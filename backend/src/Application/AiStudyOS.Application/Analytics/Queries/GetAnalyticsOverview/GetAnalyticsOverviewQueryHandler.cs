using AiStudyOS.Application.Analytics.Ai;
using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Analytics.Queries.GetAnalyticsOverview;

/// <summary>
/// The From/To range filters the time-series sections (task stats, daily activity, AI usage) — the
/// pieces that genuinely mean something over "the selected period." Goals/Streak/Mastery/Mentor
/// reflect current state regardless of range (a "current streak" or "total goals" scoped to an
/// arbitrary custom range isn't a coherent question), and Study Time is always its own fixed
/// today/7-day/30-day triple, matching the Dashboard's identical fields.
/// </summary>
public class GetAnalyticsOverviewQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator,
    IOptions<AnalyticsOptions> options,
    ILogger<GetAnalyticsOverviewQueryHandler> logger) : IQueryHandler<GetAnalyticsOverviewQuery, AnalyticsOverviewDto>
{
    public async ValueTask<AnalyticsOverviewDto> Handle(GetAnalyticsOverviewQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        var to = query.To ?? today;
        var from = query.From ?? to.AddDays(-(options.Value.MonthlyWindowDays - 1));

        var studyTime = await AnalyticsQueryHelpers.ComputeStudyTimeAsync(db, userId, today, ct);
        var tasks = await AnalyticsQueryHelpers.ComputeTaskStatsAsync(db, userId, from, to, ct);
        var goals = await AnalyticsQueryHelpers.ComputeGoalAnalyticsAsync(db, userId, ct);
        var streak = await AnalyticsQueryHelpers.ComputeStreakAnalyticsAsync(db, userId, today, options.Value, ct);
        var quizzes = await AnalyticsQueryHelpers.ComputeQuizAnalyticsAsync(db, userId, ct);
        var mastery = await AnalyticsQueryHelpers.ComputeMasteryAnalyticsAsync(db, userId, options.Value, ct);
        var mentor = await AnalyticsQueryHelpers.ComputeMentorAnalyticsAsync(db, userId, ct);
        var ai = await AnalyticsQueryHelpers.ComputeAiAnalyticsAsync(db, userId, from, to, ct);
        var planner = await AnalyticsQueryHelpers.ComputePlannerAnalyticsAsync(db, userId, ct);
        var timeline = await AnalyticsQueryHelpers.ComputeTimelineAsync(db, userId, options.Value.TimelineTake, ct);
        var taskDistribution = await AnalyticsQueryHelpers.ComputeTaskStatusDistributionAsync(db, userId, from, to, ct);
        var insights = await InsightsQueryHelper.GetOrGenerateAsync(db, mediator, dateTimeProvider, logger, userId, ct);

        return new AnalyticsOverviewDto(from, to, studyTime, tasks, goals, streak, quizzes, mastery, mentor, ai, planner, timeline, taskDistribution, insights);
    }
}
