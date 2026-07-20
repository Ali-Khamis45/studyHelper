using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Telemetry;

/// <summary>
/// A seam for future metrics (Prometheus, OpenTelemetry Metrics) — AiKernel calls these on every
/// execution alongside IAiTelemetryRecorder. The current implementation (NoOpAiMetrics) does
/// nothing; swapping in a real backend later means adding one new IAiMetrics implementation and
/// changing its DI registration, no caller changes.
/// </summary>
public interface IAiMetrics
{
    void IncrementGeneration(string providerKey, AgentType agentType, bool stream);
    void IncrementFailure(string providerKey, AgentType agentType, string errorType);
    void IncrementCancellation(string providerKey, AgentType agentType);
    void ObserveLatency(string providerKey, AgentType agentType, bool stream, long latencyMs);
}
