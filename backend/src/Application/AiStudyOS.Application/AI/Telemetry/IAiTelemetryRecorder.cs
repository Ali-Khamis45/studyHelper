using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Telemetry;

public record AiTelemetryRecord(
    string CorrelationId,
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
    DateTime CreatedAtUtc,
    bool Stream = false,
    bool Cached = false,
    string? CircuitBreakerState = null,
    long? ResponseSizeBytes = null,
    string? CancellationReason = null,
    Guid? UserId = null);

public interface IAiTelemetryRecorder
{
    Task RecordAsync(AiTelemetryRecord record, CancellationToken ct);
}
