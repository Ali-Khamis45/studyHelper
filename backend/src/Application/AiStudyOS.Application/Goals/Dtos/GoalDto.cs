using AiStudyOS.Domain.Goals;

namespace AiStudyOS.Application.Goals.Dtos;

public record GoalDto(
    Guid Id,
    string Title,
    string? Description,
    string Category,
    string Status,
    string Priority,
    DateOnly? TargetDate,
    int ProgressPercent,
    int TotalTasks,
    int CompletedTasks,
    DateTime CreatedAtUtc)
{
    public static GoalDto FromDomain(Goal goal, int totalTasks, int completedTasks)
    {
        var progress = totalTasks == 0 ? 0 : (int)Math.Round(completedTasks * 100.0 / totalTasks);

        return new GoalDto(
            goal.Id,
            goal.Title,
            goal.Description,
            goal.Category.ToString(),
            goal.Status.ToString(),
            goal.Priority.ToString(),
            goal.TargetDate,
            progress,
            totalTasks,
            completedTasks,
            goal.CreatedAtUtc);
    }
}
