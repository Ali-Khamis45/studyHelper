using AiStudyOS.Domain.Common;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Domain.Telemetry;

/// <summary>
/// Append-only record of a single IAiKernel execution (success or failure). Factory takes
/// primitives rather than the Application-layer AiTelemetryRecord DTO — Domain must not
/// reference Application; the mapping happens in Infrastructure's PostgresAiTelemetryRecorder.
/// </summary>
public class AiTelemetryEvent : Entity
{
    public string CorrelationId { get; private set; } = null!;
    public AgentType AgentType { get; private set; }
    public string ProviderKey { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public string? PromptVersion { get; private set; }
    public int PromptTokens { get; private set; }
    public int CompletionTokens { get; private set; }
    public decimal EstimatedCostUsd { get; private set; }
    public long LatencyMs { get; private set; }
    public int RetryCount { get; private set; }
    public int JsonRepairCount { get; private set; }
    public int ToolCallCount { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorType { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool Stream { get; private set; }
    public bool Cached { get; private set; }
    public string? CircuitBreakerState { get; private set; }
    public long? ResponseSizeBytes { get; private set; }
    public string? CancellationReason { get; private set; }

    private AiTelemetryEvent() { }

    public static AiTelemetryEvent Create(
        string correlationId,
        AgentType agentType,
        string providerKey,
        string model,
        string? promptVersion,
        int promptTokens,
        int completionTokens,
        decimal estimatedCostUsd,
        long latencyMs,
        int retryCount,
        int jsonRepairCount,
        int toolCallCount,
        bool success,
        string? errorType,
        DateTime createdAtUtc,
        bool stream,
        bool cached,
        string? circuitBreakerState,
        long? responseSizeBytes,
        string? cancellationReason)
    {
        return new AiTelemetryEvent
        {
            CorrelationId = correlationId,
            AgentType = agentType,
            ProviderKey = providerKey,
            Model = model,
            PromptVersion = promptVersion,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            EstimatedCostUsd = estimatedCostUsd,
            LatencyMs = latencyMs,
            RetryCount = retryCount,
            JsonRepairCount = jsonRepairCount,
            ToolCallCount = toolCallCount,
            Success = success,
            ErrorType = errorType,
            CreatedAtUtc = createdAtUtc,
            Stream = stream,
            Cached = cached,
            CircuitBreakerState = circuitBreakerState,
            ResponseSizeBytes = responseSizeBytes,
            CancellationReason = cancellationReason,
        };
    }
}
