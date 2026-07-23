namespace AiStudyOS.Application.Analytics.Ai;

public record InsightsResult(
    string WeeklySummary,
    string MonthlySummary,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> RecommendedFocusAreas,
    string RiskDetection,
    IReadOnlyList<string> SuggestedScheduleImprovements);
