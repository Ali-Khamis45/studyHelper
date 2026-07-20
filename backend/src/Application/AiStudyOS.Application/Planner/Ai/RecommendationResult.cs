namespace AiStudyOS.Application.Planner.Ai;

// The structured JSON contract the Recommendation agent's prompt (Infrastructure/AI/Prompts/Recommendation/v1.md)
// asks the model to return — AiKernel deserializes directly into this.

public record RecommendationResult(
    string SituationAnalysis,
    string GoalAlignment,
    string Evidence,
    string Recommendation,
    string ImmediateNextAction,
    double ConfidenceScore,
    string RecommendationReason,
    IReadOnlyList<RecommendedTaskResult> Tasks);

public record RecommendedTaskResult(Guid? GoalId, string Title, string Reasoning, int EstimatedMinutes, string? EnergyLevel);
