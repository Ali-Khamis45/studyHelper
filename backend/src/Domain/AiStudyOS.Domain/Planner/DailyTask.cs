using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Planner;

public class DailyTask : Entity
{
    public Guid UserId { get; private set; }
    public Guid? GoalId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Reasoning { get; private set; }
    public DateOnly Date { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public DailyTaskStatus Status { get; private set; }
    public TaskSource Source { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private DailyTask() { }

    public static DailyTask Create(
        Guid userId,
        Guid? goalId,
        string title,
        string? reasoning,
        DateOnly date,
        int estimatedMinutes,
        TaskSource source,
        DateTime nowUtc)
    {
        return new DailyTask
        {
            UserId = userId,
            GoalId = goalId,
            Title = title,
            Reasoning = reasoning,
            Date = date,
            EstimatedMinutes = estimatedMinutes,
            Status = DailyTaskStatus.Pending,
            Source = source,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
        };
    }

    public void Complete(DateTime nowUtc)
    {
        Status = DailyTaskStatus.Completed;
        CompletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void Skip(DateTime nowUtc)
    {
        Status = DailyTaskStatus.Skipped;
        CompletedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    public void Reschedule(DateOnly newDate, DateTime nowUtc)
    {
        Date = newDate;
        Status = DailyTaskStatus.Pending;
        CompletedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }
}
