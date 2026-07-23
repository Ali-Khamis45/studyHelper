using AiStudyOS.Application.Analytics.Dtos.Charts;
using AiStudyOS.Application.Quiz.Dtos;

namespace AiStudyOS.Application.Analytics.Dtos;

/// <summary>
/// The Dashboard's own light bundle — deliberately doesn't duplicate data other widgets already
/// have a dedicated, cheaper source for (today's plan/streak/focus score come from Planner's
/// existing GetTodayQuery, today's Mentor conversation from Mentor's own list endpoint) — only the
/// pieces genuinely unique to Analytics.
/// </summary>
public record DashboardAnalyticsDto(
    IReadOnlyList<TopicMasteryDto> WeakTopics,
    IReadOnlyList<ChartPointDto> WeeklyActivity,
    GoalAnalyticsDto Goals,
    InsightsDto? Insights);
