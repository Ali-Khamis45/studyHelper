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
    DateTime? CompletedAtUtc)
{
    public static DailyTaskDto FromDomain(DailyTask task, string? goalTitle) => new(
        task.Id,
        task.GoalId,
        goalTitle,
        task.Title,
        task.Reasoning,
        task.Date,
        task.EstimatedMinutes,
        task.Status.ToString(),
        task.Source.ToString(),
        task.CompletedAtUtc);
}
