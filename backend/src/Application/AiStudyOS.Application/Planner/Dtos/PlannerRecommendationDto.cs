using AiStudyOS.Domain.Planner;

namespace AiStudyOS.Application.Planner.Dtos;

public record PlannerRecommendationDto(
    Guid Id,
    DateOnly Date,
    string SituationAnalysis,
    string GoalAlignment,
    string Evidence,
    string Recommendation,
    string ImmediateNextAction,
    string ModelUsed,
    string Provider,
    string? PromptVersion,
    double ConfidenceScore,
    string RecommendationReason,
    long GenerationTimeMs,
    DateTime GeneratedAt,
    DateTime ExpiresAt)
{
    public static PlannerRecommendationDto FromDomain(PlannerRecommendation recommendation) => new(
        recommendation.Id,
        recommendation.Date,
        recommendation.SituationAnalysis,
        recommendation.GoalAlignment,
        recommendation.Evidence,
        recommendation.Recommendation,
        recommendation.ImmediateNextAction,
        recommendation.ModelUsed,
        recommendation.Provider,
        recommendation.PromptVersion,
        recommendation.ConfidenceScore,
        recommendation.RecommendationReason,
        recommendation.GenerationTimeMs,
        recommendation.GeneratedAt,
        recommendation.ExpiresAt);
}
