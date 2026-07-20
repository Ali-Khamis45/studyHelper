using AiStudyOS.Domain.Planner;

namespace AiStudyOS.Application.Planner.Dtos;

public record DailyTaskDto(
    Guid Id,
    Guid? GoalId,
    string? GoalTitle,
    string Title,
    string? Reasoning,
    DateOnly Date,
    int EstimatedMinutes,
    string Status,
    string Source,
    string? EnergyLevel,
    bool IsOverdue,
    DateTime? CompletedAtUtc)
{
    public static DailyTaskDto FromDomain(DailyTask task, string? goalTitle, DateOnly today) => new(
        task.Id,
        task.GoalId,
        goalTitle,
        task.Title,
        task.Reasoning,
        task.Date,
        task.EstimatedMinutes,
        task.Status.ToString(),
        task.Source.ToString(),
        task.EnergyLevel?.ToString(),
        task.Status == DailyTaskStatus.Pending && task.Date < today,
        task.CompletedAtUtc);
}
