using AiStudyOS.Application.AI.Telemetry;
using Microsoft.Extensions.Logging;

namespace AiStudyOS.Infrastructure.AI.Telemetry;

/// <summary>
/// Real telemetry — every AiKernel execution is recorded via structured logging (queryable through
/// whatever log sink is configured). Not a Postgres table yet (that's a drop-in swap behind the
/// same IAiTelemetryRecorder interface, deferred until there's an actual ops/cost dashboard to feed).
/// </summary>
public class LoggingAiTelemetryRecorder(ILogger<LoggingAiTelemetryRecorder> logger) : IAiTelemetryRecorder
{
    public Task RecordAsync(AiTelemetryRecord record, CancellationToken ct)
    {
        if (record.Success)
        {
            logger.LogInformation(
                "AI {AgentType} via {ProviderKey}/{Model} succeeded in {LatencyMs}ms (stream={Stream}, prompt={PromptTokens}t, completion={CompletionTokens}t, retries={RetryCount}, repairs={JsonRepairCount}, circuit={CircuitBreakerState}) [{CorrelationId}]",
                record.AgentType, record.ProviderKey, record.Model, record.LatencyMs, record.Stream, record.PromptTokens, record.CompletionTokens, record.RetryCount, record.JsonRepairCount, record.CircuitBreakerState, record.CorrelationId);
        }
        else if (record.ErrorType == "Cancelled")
        {
            // Cancellation is not a failure — the client simply stopped waiting. Logged at
            // Information, not Warning, and without the word "FAILED".
            logger.LogInformation(
                "AI {AgentType} via {ProviderKey}/{Model} cancelled after {LatencyMs}ms (stream={Stream}, retries={RetryCount}, reason={CancellationReason}) [{CorrelationId}]",
                record.AgentType, record.ProviderKey, record.Model, record.LatencyMs, record.Stream, record.RetryCount, record.CancellationReason, record.CorrelationId);
        }
        else
        {
            logger.LogWarning(
                "AI {AgentType} via {ProviderKey}/{Model} FAILED after {LatencyMs}ms ({ErrorType}, stream={Stream}, retries={RetryCount}, repairs={JsonRepairCount}, circuit={CircuitBreakerState}) [{CorrelationId}]",
                record.AgentType, record.ProviderKey, record.Model, record.LatencyMs, record.ErrorType, record.Stream, record.RetryCount, record.JsonRepairCount, record.CircuitBreakerState, record.CorrelationId);
        }

        return Task.CompletedTask;
    }
}
