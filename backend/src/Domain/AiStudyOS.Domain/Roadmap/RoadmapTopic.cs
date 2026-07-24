using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Roadmap;

/// <summary>
/// Its own table (like QuizQuestion under Quiz), queried by RoadmapId rather than loaded as a
/// navigation collection. ParentTopicId is a self-reference, giving the tree unlimited nesting
/// depth from one flat table — a null parent is a top-level "section" node; everything else is a
/// topic or sub-topic under it. Status is deliberately NOT stored here: it's computed at read time
/// from ManuallyCompletedAtUtc, PrerequisiteTopicIds, and the user's live TopicMastery rows (see
/// RoadmapProgressCalculator), so completing a quiz never needs this table to be written to.
/// </summary>
public class RoadmapTopic : AggregateRoot
{
    public Guid RoadmapId { get; private set; }
    public Guid? ParentTopicId { get; private set; }
    public int Order { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public double EstimatedHours { get; private set; }
    public RoadmapDifficultyLevel Difficulty { get; private set; }

    /// <summary>JSON array of this topic's own prerequisite RoadmapTopic ids (resolved from the AI's title references at generation time).</summary>
    public string PrerequisiteTopicIdsJson { get; private set; } = "[]";

    /// <summary>JSON array of {type,title,url} resource items.</summary>
    public string ResourcesJson { get; private set; } = "[]";

    /// <summary>JSON array of suggested project title strings.</summary>
    public string SuggestedProjectsJson { get; private set; } = "[]";

    /// <summary>The Topic Mastery topic name this node is scored against — defaults to Title, so Quiz's existing per-topic mastery tracking drives this node's progress with no new tables.</summary>
    public string LinkedMasteryTopic { get; private set; } = null!;

    public DateTime? ManuallyCompletedAtUtc { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private RoadmapTopic() { }

    public static RoadmapTopic Create(
        Guid roadmapId,
        Guid? parentTopicId,
        int order,
        string title,
        string description,
        double estimatedHours,
        RoadmapDifficultyLevel difficulty,
        string resourcesJson,
        string suggestedProjectsJson,
        DateTime nowUtc) => new()
    {
        RoadmapId = roadmapId,
        ParentTopicId = parentTopicId,
        Order = order,
        Title = title,
        Description = description,
        EstimatedHours = estimatedHours,
        Difficulty = difficulty,
        ResourcesJson = resourcesJson,
        SuggestedProjectsJson = suggestedProjectsJson,
        LinkedMasteryTopic = title,
        CreatedAtUtc = nowUtc,
        UpdatedAtUtc = nowUtc,
    };

    public void SetPrerequisites(string prerequisiteTopicIdsJson, DateTime nowUtc)
    {
        PrerequisiteTopicIdsJson = prerequisiteTopicIdsJson;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkComplete(DateTime nowUtc)
    {
        ManuallyCompletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkIncomplete(DateTime nowUtc)
    {
        ManuallyCompletedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    public void UpdateNotes(string? notes, DateTime nowUtc)
    {
        Notes = notes;
        UpdatedAtUtc = nowUtc;
    }
}
