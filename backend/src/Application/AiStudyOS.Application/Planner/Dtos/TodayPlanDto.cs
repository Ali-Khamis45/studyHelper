namespace AiStudyOS.Application.Planner.Dtos;

public record UpcomingDeadlineDto(Guid GoalId, string Title, DateOnly TargetDate, int DaysRemaining);

public record TodayPlanDto(
    PlannerRecommendationDto? Recommendation,
    IReadOnlyList<DailyTaskDto> Tasks,
    IReadOnlyList<UpcomingDeadlineDto> UpcomingDeadlines);

public record WeekDayDto(DateOnly Date, IReadOnlyList<DailyTaskDto> Tasks);

public record WeekDto(IReadOnlyList<WeekDayDto> Days);
