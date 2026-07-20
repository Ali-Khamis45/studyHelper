using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Infrastructure.AI.Telemetry;

/// <summary>No metrics backend exists yet — this is purely the seam described on IAiMetrics.</summary>
public class NoOpAiMetrics : IAiMetrics
{
    public void IncrementGeneration(string providerKey, AgentType agentType, bool stream) { }
    public void IncrementFailure(string providerKey, AgentType agentType, string errorType) { }
    public void IncrementCancellation(string providerKey, AgentType agentType) { }
    public void ObserveLatency(string providerKey, AgentType agentType, bool stream, long latencyMs) { }
}
