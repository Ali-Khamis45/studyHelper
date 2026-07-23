using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Kernel;

public record KernelRequest(
    AgentType AgentType,
    PromptDefinition Prompt,
    AiContext Context,
    string? ExpectedSchemaJson = null,
    string? ModelOverride = null,
    string? UserMessage = null,
    Guid? UserId = null);

public record KernelResult<T>(T? Data, bool Success, string RawContent, AiTelemetryRecord Telemetry, IReadOnlyList<string> Errors);

/// <summary>
/// One chunk of a streaming execution. Non-final chunks only ever carry DeltaContent. The final
/// chunk carries a KernelResult&lt;T&gt; — the exact same shape ExecuteAsync returns — because
/// streaming and non-streaming go through the same parse/validate/telemetry pipeline internally;
/// the only real difference is that streaming also yields text as it arrives.
/// </summary>
public record KernelStreamChunk<T>(string DeltaContent, bool IsFinal, KernelResult<T>? Result = null);

public record AiHealthResult(string Provider, string Model, bool IsHealthy, long LatencyMs, string? Message);

/// <summary>
/// The only component in Application/Infrastructure that ever talks to an AI provider adapter.
/// Agents call this, never IAiChatClient directly, so retry/JSON-repair/schema-validation/telemetry
/// live in one place instead of being duplicated per agent.
/// </summary>
public interface IAiKernel
{
    Task<KernelResult<T>> ExecuteAsync<T>(KernelRequest request, CancellationToken ct);
    IAsyncEnumerable<KernelStreamChunk<T>> ExecuteStreamAsync<T>(KernelRequest request, CancellationToken ct);

    /// <summary>
    /// Lightweight real connectivity check against the configured provider — does not invoke the
    /// model. Never throws: failures surface as a non-healthy AiHealthResult.
    /// </summary>
    Task<AiHealthResult> CheckHealthAsync(CancellationToken ct);
}
