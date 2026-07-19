using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Telemetry;

public record AiTelemetryRecord(
    Guid CorrelationId,
    AgentType AgentType,
    string ProviderKey,
    string Model,
    string? PromptVersion,
    int PromptTokens,
    int CompletionTokens,
    decimal EstimatedCostUsd,
    long LatencyMs,
    int RetryCount,
    int JsonRepairCount,
    int ToolCallCount,
    bool Success,
    string? ErrorType,
    DateTime CreatedAtUtc);

public interface IAiTelemetryRecorder
{
    Task RecordAsync(AiTelemetryRecord record, CancellationToken ct);
}
