using AiStudyOS.Application.Analytics.Dtos.Charts;

namespace AiStudyOS.Application.Analytics.Dtos;

/// <summary>The full report behind /analytics (range-filterable) and both export formats — every other endpoint's DTO is a narrower slice of this same data.</summary>
public record AnalyticsOverviewDto(
    DateOnly From,
    DateOnly To,
    StudyTimeStatsDto StudyTime,
    TaskStatsDto Tasks,
    GoalAnalyticsDto Goals,
    StreakAnalyticsDto Streak,
    QuizAnalyticsDto Quizzes,
    MasteryAnalyticsDto Mastery,
    MentorAnalyticsDto Mentor,
    AiAnalyticsDto Ai,
    PlannerAnalyticsDto Planner,
    IReadOnlyList<TimelineEventDto> Timeline,
    IReadOnlyList<PieSliceDto> TaskStatusDistribution,
    InsightsDto? Insights);
