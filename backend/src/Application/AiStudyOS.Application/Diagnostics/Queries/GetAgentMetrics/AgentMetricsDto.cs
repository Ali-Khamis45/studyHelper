using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.Diagnostics.Queries.GetAgentMetrics;

public record AgentMetricsDto(
    AgentType AgentType,
    int TotalExecutions,
    int SuccessCount,
    int FailureCount,
    double AverageLatencyMs,
    double RetryRatePercent,
    double AverageTotalTokens,
    DateTime LastExecutionUtc);
