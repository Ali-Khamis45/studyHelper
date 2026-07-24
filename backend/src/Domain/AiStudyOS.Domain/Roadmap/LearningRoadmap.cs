using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Roadmap;

public class LearningRoadmap : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string CareerGoal { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public RoadmapDifficultyLevel Difficulty { get; private set; }
    public int EstimatedWeeks { get; private set; }
    public RoadmapStatus Status { get; private set; }
    public string ModelUsed { get; private set; } = null!;
    public string PromptVersion { get; private set; } = null!;
    public string CorrelationId { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private LearningRoadmap() { }

    public static LearningRoadmap Create(
        Guid userId,
        string careerGoal,
        string title,
        string description,
        RoadmapDifficultyLevel difficulty,
        int estimatedWeeks,
        string modelUsed,
        string promptVersion,
        string correlationId,
        DateTime nowUtc) => new()
    {
        UserId = userId,
        CareerGoal = careerGoal,
        Title = title,
        Description = description,
        Difficulty = difficulty,
        EstimatedWeeks = estimatedWeeks,
        Status = RoadmapStatus.Active,
        ModelUsed = modelUsed,
        PromptVersion = promptVersion,
        CorrelationId = correlationId,
        CreatedAtUtc = nowUtc,
        UpdatedAtUtc = nowUtc,
    };

    public void MarkCompleted(DateTime nowUtc)
    {
        Status = RoadmapStatus.Completed;
        UpdatedAtUtc = nowUtc;
    }

    public void Archive(DateTime nowUtc)
    {
        Status = RoadmapStatus.Archived;
        UpdatedAtUtc = nowUtc;
    }

    public void Touch(DateTime nowUtc) => UpdatedAtUtc = nowUtc;
}
