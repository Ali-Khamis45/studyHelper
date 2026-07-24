namespace AiStudyOS.Application.Roadmap.Dtos;

public record RoadmapResourceDto(string Type, string Title, string? Url);

public record PrerequisiteStatusDto(Guid TopicId, string Title, bool Met);

public record RoadmapTopicDto(
    Guid Id,
    Guid? ParentTopicId,
    int Order,
    string Title,
    string Description,
    double EstimatedHours,
    string Difficulty,
    IReadOnlyList<RoadmapResourceDto> Resources,
    IReadOnlyList<string> SuggestedProjects,
    IReadOnlyList<PrerequisiteStatusDto> Prerequisites,
    string LinkedMasteryTopic,
    double MasteryScore,
    string Status,
    bool ManuallyCompleted,
    string? Notes,
    DateTime UpdatedAtUtc,
    IReadOnlyList<RoadmapTopicDto> Children);

public record RoadmapDto(
    Guid Id,
    string CareerGoal,
    string Title,
    string Description,
    string Difficulty,
    int EstimatedWeeks,
    string Status,
    double ProgressPercent,
    int CompletedTopicCount,
    int TotalTopicCount,
    double TotalEstimatedHours,
    double RemainingEstimatedHours,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<RoadmapTopicDto> Sections);

public record RoadmapSummaryDto(
    Guid Id,
    string CareerGoal,
    string Title,
    string Difficulty,
    int EstimatedWeeks,
    string Status,
    double ProgressPercent,
    int CompletedTopicCount,
    int TotalTopicCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
