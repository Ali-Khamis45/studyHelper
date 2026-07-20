using AiStudyOS.Domain.Common;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Domain.Planner;

public class PlannerRecommendation : Entity
{
    public Guid UserId { get; private set; }
    public DateOnly Date { get; private set; }
    public string SituationAnalysis { get; private set; } = null!;
    public string GoalAlignment { get; private set; } = null!;
    public string Evidence { get; private set; } = null!;
    public string Recommendation { get; private set; } = null!;
    public string ImmediateNextAction { get; private set; } = null!;
    public Guid? RecommendedTaskId { get; private set; }
    public AgentType AgentType { get; private set; }

    // AI-sourced metadata (§5) — populated from the kernel execution whenever possible.
    public string ModelUsed { get; private set; } = null!;
    public string Provider { get; private set; } = null!;
    public string? PromptVersion { get; private set; }
    public double ConfidenceScore { get; private set; }
    public string RecommendationReason { get; private set; } = null!;
    public long GenerationTimeMs { get; private set; }
    public string RawResponseJson { get; private set; } = null!;

    // Cache semantics (§2).
    public DateTime GeneratedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? InvalidatedAt { get; private set; }

    private PlannerRecommendation() { }

    public static PlannerRecommendation Create(
        Guid userId,
        DateOnly date,
        string situationAnalysis,
        string goalAlignment,
        string evidence,
        string recommendation,
        string immediateNextAction,
        Guid? recommendedTaskId,
        string modelUsed,
        string provider,
        string? promptVersion,
        double confidenceScore,
        string recommendationReason,
        long generationTimeMs,
        string rawResponseJson,
        DateTime nowUtc)
    {
        return new PlannerRecommendation
        {
            UserId = userId,
            Date = date,
            SituationAnalysis = situationAnalysis,
            GoalAlignment = goalAlignment,
            Evidence = evidence,
            Recommendation = recommendation,
            ImmediateNextAction = immediateNextAction,
            RecommendedTaskId = recommendedTaskId,
            AgentType = AgentType.Recommendation,
            ModelUsed = modelUsed,
            Provider = provider,
            PromptVersion = promptVersion,
            ConfidenceScore = confidenceScore,
            RecommendationReason = recommendationReason,
            GenerationTimeMs = generationTimeMs,
            RawResponseJson = rawResponseJson,
            GeneratedAt = nowUtc,
            // Valid through the end of the recommendation's date — "today's" recommendation
            // naturally expires at midnight even if nothing ever explicitly invalidates it.
            ExpiresAt = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc),
        };
    }

    public bool IsActive(DateTime nowUtc) => InvalidatedAt is null && ExpiresAt > nowUtc;

    public void Invalidate(DateTime nowUtc) => InvalidatedAt ??= nowUtc;
}
