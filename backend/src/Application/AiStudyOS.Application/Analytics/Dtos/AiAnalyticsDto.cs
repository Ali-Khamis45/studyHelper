namespace AiStudyOS.Application.Analytics.Dtos;

public record ProviderStatDto(string Provider, int RequestCount, double AverageLatencyMs);

public record AiAnalyticsDto(
    int TotalRequests,
    double SuccessRatePercent,
    double FailureRatePercent,
    double AverageLatencyMs,
    int TotalPromptTokens,
    int TotalCompletionTokens,
    IReadOnlyList<ProviderStatDto> ByProvider);
