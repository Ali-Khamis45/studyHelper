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
    string? ModelOverride = null);

public record KernelResult<T>(T? Data, bool Success, string RawContent, AiTelemetryRecord Telemetry, IReadOnlyList<string> Errors);

public record KernelStreamChunk(string DeltaContent, bool IsFinal);

/// <summary>
/// The only component in Application/Infrastructure that ever talks to an AI provider adapter.
/// Agents call this, never IAiChatClient directly, so retry/JSON-repair/schema-validation/telemetry
/// live in one place instead of being duplicated per agent.
/// </summary>
public interface IAiKernel
{
    Task<KernelResult<T>> ExecuteAsync<T>(KernelRequest request, CancellationToken ct);
    IAsyncEnumerable<KernelStreamChunk> ExecuteStreamAsync(KernelRequest request, CancellationToken ct);
}
