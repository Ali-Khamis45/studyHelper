using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Telemetry;

namespace AiStudyOS.Infrastructure.AI.Telemetry;

/// <summary>
/// Persists every AiKernel execution to ai_telemetry_events so it survives restarts. Commits
/// independently via its own SaveChangesAsync — telemetry for a failed generation must persist
/// even if the rest of that request's work never gets that far.
/// </summary>
public class PostgresAiTelemetryRecorder(IApplicationDbContext db) : IAiTelemetryRecorder
{
    public async Task RecordAsync(AiTelemetryRecord record, CancellationToken ct)
    {
        var entity = AiTelemetryEvent.Create(
            record.CorrelationId,
            record.AgentType,
            record.ProviderKey,
            record.Model,
            record.PromptVersion,
            record.PromptTokens,
            record.CompletionTokens,
            record.EstimatedCostUsd,
            record.LatencyMs,
            record.RetryCount,
            record.JsonRepairCount,
            record.ToolCallCount,
            record.Success,
            record.ErrorType,
            record.CreatedAtUtc,
            record.Stream,
            record.Cached,
            record.CircuitBreakerState,
            record.ResponseSizeBytes,
            record.CancellationReason,
            record.UserId);

        db.AiTelemetryEvents.Add(entity);
        await db.SaveChangesAsync(ct);
    }
}
