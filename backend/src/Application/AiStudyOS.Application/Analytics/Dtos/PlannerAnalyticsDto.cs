using AiStudyOS.Application.Analytics.Dtos.Charts;

namespace AiStudyOS.Application.Analytics.Dtos;

/// <summary>AcceptanceRatePercent: among recommendations that proposed a specific task, the share of those tasks the student actually completed — a real, observable proxy for "did the student follow the AI's suggestion," not a self-reported rating (no such feature exists).</summary>
public record PlannerAnalyticsDto(
    int RecommendationCount,
    double AcceptanceRatePercent,
    double AverageConfidence,
    double AverageGenerationTimeMs,
    IReadOnlyList<ChartPointDto> GenerationTrend);
