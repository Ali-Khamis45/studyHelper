namespace AiStudyOS.Application.Analytics.Dtos;

public record GoalProgressItemDto(Guid Id, string Title, string Status, int ProgressPercent);

public record GoalAnalyticsDto(int TotalGoals, int CompletedGoals, double CompletionPercent, IReadOnlyList<GoalProgressItemDto> Goals);
