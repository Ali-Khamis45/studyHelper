using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Analytics;

/// <summary>
/// A cached AI-generated insight report — real Ollama generation takes seconds, so the Dashboard
/// widget and the /analytics AI Insights section reuse one report for a day rather than
/// regenerating on every page load, exactly mirroring PlannerRecommendation's cache-with-expiry
/// pattern (IsActive / Invalidate / ExpiresAt).
/// </summary>
public class AnalyticsInsight : Entity
{
    public Guid UserId { get; private set; }
    public string WeeklySummary { get; private set; } = null!;
    public string MonthlySummary { get; private set; } = null!;

    /// <summary>JSON string arrays — same convention as QuizQuestion.OptionsJson.</summary>
    public string StrengthsJson { get; private set; } = null!;
    public string WeaknessesJson { get; private set; } = null!;
    public string RecommendedFocusAreasJson { get; private set; } = null!;
    public string SuggestedScheduleImprovementsJson { get; private set; } = null!;
    public string RiskDetection { get; private set; } = null!;

    public string ModelUsed { get; private set; } = null!;
    public string? PromptVersion { get; private set; }
    public string CorrelationId { get; private set; } = null!;

    public DateTime GeneratedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? InvalidatedAtUtc { get; private set; }

    private AnalyticsInsight() { }

    public static AnalyticsInsight Create(
        Guid userId,
        string weeklySummary,
        string monthlySummary,
        string strengthsJson,
        string weaknessesJson,
        string recommendedFocusAreasJson,
        string suggestedScheduleImprovementsJson,
        string riskDetection,
        string modelUsed,
        string? promptVersion,
        string correlationId,
        DateTime nowUtc,
        TimeSpan validFor) => new()
    {
        UserId = userId,
        WeeklySummary = weeklySummary,
        MonthlySummary = monthlySummary,
        StrengthsJson = strengthsJson,
        WeaknessesJson = weaknessesJson,
        RecommendedFocusAreasJson = recommendedFocusAreasJson,
        SuggestedScheduleImprovementsJson = suggestedScheduleImprovementsJson,
        RiskDetection = riskDetection,
        ModelUsed = modelUsed,
        PromptVersion = promptVersion,
        CorrelationId = correlationId,
        GeneratedAtUtc = nowUtc,
        ExpiresAtUtc = nowUtc.Add(validFor),
    };

    public bool IsActive(DateTime nowUtc) => InvalidatedAtUtc is null && ExpiresAtUtc > nowUtc;

    public void Invalidate(DateTime nowUtc) => InvalidatedAtUtc ??= nowUtc;
}
