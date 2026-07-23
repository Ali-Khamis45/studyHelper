namespace AiStudyOS.Application.Analytics.Dtos;

public record InsightsDto(
    string WeeklySummary,
    string MonthlySummary,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> RecommendedFocusAreas,
    string RiskDetection,
    IReadOnlyList<string> SuggestedScheduleImprovements,
    DateTime GeneratedAtUtc);
