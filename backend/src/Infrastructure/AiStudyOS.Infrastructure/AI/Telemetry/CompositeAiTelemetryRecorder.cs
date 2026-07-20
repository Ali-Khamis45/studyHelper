using AiStudyOS.Application.AI.Telemetry;

namespace AiStudyOS.Infrastructure.AI.Telemetry;

/// <summary>
/// Fans out to both the durable (Postgres) and the immediate-visibility (log) recorders, so
/// telemetry survives restarts and is still visible in real time in the console/log sink.
/// </summary>
public class CompositeAiTelemetryRecorder(PostgresAiTelemetryRecorder postgres, LoggingAiTelemetryRecorder logging) : IAiTelemetryRecorder
{
    public async Task RecordAsync(AiTelemetryRecord record, CancellationToken ct)
    {
        await postgres.RecordAsync(record, ct);
        await logging.RecordAsync(record, ct);
    }
}
