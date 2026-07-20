using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Goals;

public class Goal : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public GoalCategory Category { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public GoalStatus Status { get; private set; }
    public GoalPriority Priority { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Goal() { }

    public static Goal Create(
        Guid userId,
        string title,
        string? description,
        GoalCategory category,
        GoalPriority priority,
        DateOnly? targetDate,
        DateTime nowUtc)
    {
        return new Goal
        {
            UserId = userId,
            Title = title,
            Description = description,
            Category = category,
            Priority = priority,
            TargetDate = targetDate,
            Status = GoalStatus.Active,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
        };
    }

    public void Update(string title, string? description, GoalCategory category, GoalPriority priority, DateOnly? targetDate, DateTime nowUtc)
    {
        Title = title;
        Description = description;
        Category = category;
        Priority = priority;
        TargetDate = targetDate;
        UpdatedAtUtc = nowUtc;
    }

    public void SetStatus(GoalStatus status, DateTime nowUtc)
    {
        Status = status;
        UpdatedAtUtc = nowUtc;
    }
}
