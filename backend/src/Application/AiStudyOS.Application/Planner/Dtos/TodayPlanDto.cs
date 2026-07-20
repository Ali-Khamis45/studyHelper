namespace AiStudyOS.Application.Planner.Dtos;

public record UpcomingDeadlineDto(Guid GoalId, string Title, DateOnly TargetDate, int DaysRemaining);

public record TodayPlanDto(
    PlannerRecommendationDto? Recommendation,
    IReadOnlyList<DailyTaskDto> Tasks,
    IReadOnlyList<UpcomingDeadlineDto> UpcomingDeadlines,
    IReadOnlyList<DailyTaskDto> OverdueTasks,
    double DailyCompletionPercent,
    int DailyFocusScore,
    int StudyStreak);

public record WeekDayDto(DateOnly Date, IReadOnlyList<DailyTaskDto> Tasks, int TotalEstimatedMinutes, bool IsOverloaded);

public record WeekDto(IReadOnlyList<WeekDayDto> Days, double WeeklyCompletionPercent);
