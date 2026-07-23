using System.Text.Json;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Analytics;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Analytics.Ai;

/// <summary>Turns a parsed InsightsResult into a persisted, time-boxed AnalyticsInsight and returns the resulting InsightsDto — mirrors RecommendationFinalizer/QuizFinalizer.</summary>
public static class InsightsFinalizer
{
    private static readonly TimeSpan ValidFor = TimeSpan.FromHours(24);

    public static async Task<InsightsDto> FinalizeAsync(
        IApplicationDbContext db, Guid userId, InsightsResult data, AiTelemetryRecord telemetry, DateTime nowUtc, CancellationToken ct)
    {
        var weeklySummary = RequireField(data.WeeklySummary, "weeklySummary");
        var monthlySummary = RequireField(data.MonthlySummary, "monthlySummary");
        var riskDetection = RequireField(data.RiskDetection, "riskDetection");

        // Only ever one active report per user — regenerating (manually or via expiry) replaces it.
        var previouslyActive = await db.AnalyticsInsights.Where(i => i.UserId == userId && i.InvalidatedAtUtc == null).ToListAsync(ct);
        foreach (var existing in previouslyActive)
            existing.Invalidate(nowUtc);

        var insight = AnalyticsInsight.Create(
            userId, weeklySummary, monthlySummary,
            JsonSerializer.Serialize(data.Strengths ?? []),
            JsonSerializer.Serialize(data.Weaknesses ?? []),
            JsonSerializer.Serialize(data.RecommendedFocusAreas ?? []),
            JsonSerializer.Serialize(data.SuggestedScheduleImprovements ?? []),
            riskDetection, telemetry.Model, telemetry.PromptVersion, telemetry.CorrelationId, nowUtc, ValidFor);

        db.AnalyticsInsights.Add(insight);
        await db.SaveChangesAsync(ct);

        return ToDto(insight);
    }

    public static InsightsDto ToDto(AnalyticsInsight insight) => new(
        insight.WeeklySummary,
        insight.MonthlySummary,
        JsonSerializer.Deserialize<List<string>>(insight.StrengthsJson) ?? [],
        JsonSerializer.Deserialize<List<string>>(insight.WeaknessesJson) ?? [],
        JsonSerializer.Deserialize<List<string>>(insight.RecommendedFocusAreasJson) ?? [],
        insight.RiskDetection,
        JsonSerializer.Deserialize<List<string>>(insight.SuggestedScheduleImprovementsJson) ?? [],
        insight.GeneratedAtUtc);

    private static string RequireField(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value) ? throw new AiGenerationFailedException($"The AI response was missing a required '{fieldName}' value.") : value;
}
