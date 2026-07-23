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
    public EnergyLevel? EnergyLevel { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>How many times this task has been moved via Reschedule — real, persisted count backing Analytics' "Rescheduled Tasks" metric (Date itself is overwritten on each move, so without this counter that history would be unrecoverable).</summary>
    public int RescheduleCount { get; private set; }

    private DailyTask() { }

    public static DailyTask Create(
        Guid userId,
        Guid? goalId,
        string title,
        string? reasoning,
        DateOnly date,
        int estimatedMinutes,
        TaskSource source,
        DateTime nowUtc,
        EnergyLevel? energyLevel = null)
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
            EnergyLevel = energyLevel,
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
        RescheduleCount++;
        UpdatedAtUtc = nowUtc;
    }
}
