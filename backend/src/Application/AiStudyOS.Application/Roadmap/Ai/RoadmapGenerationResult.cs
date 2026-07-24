namespace AiStudyOS.Application.Roadmap.Ai;

public record RoadmapResourceResult(string Type, string Title, string? Url);

public record RoadmapTopicResult(
    string Title,
    string Description,
    double EstimatedHours,
    string Difficulty,
    IReadOnlyList<string>? Prerequisites,
    IReadOnlyList<RoadmapResourceResult>? Resources,
    IReadOnlyList<string>? SuggestedProjects,
    IReadOnlyList<RoadmapTopicResult>? SubTopics);

public record RoadmapSectionResult(string Title, string? Description, IReadOnlyList<RoadmapTopicResult> Topics);

public record RoadmapGenerationResult(
    string Title,
    string Description,
    string Difficulty,
    int EstimatedWeeks,
    IReadOnlyList<RoadmapSectionResult> Sections);
