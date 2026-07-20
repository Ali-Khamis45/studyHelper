namespace AiStudyOS.Infrastructure.AI.Providers;

// Deliberately Infrastructure-internal: only AiKernel and the provider adapters ever see this.
// Application/Agents only ever depend on IAiKernel.

public record AiChatMessage(string Role, string Content);

public record AiChatRequest(IReadOnlyList<AiChatMessage> Messages, string Model, double Temperature = 0.3, bool JsonMode = false);

public record AiChatResponse(string Content, int PromptTokens, int CompletionTokens, string ModelUsed);

public interface IAiChatClient
{
    string ProviderKey { get; }
    Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct);
    IAsyncEnumerable<string> StreamAsync(AiChatRequest request, CancellationToken ct);

    /// <summary>
    /// Lightweight liveness check — must not invoke the model itself (no token cost, fast even
    /// under load). Returns false rather than throwing when the provider can't be reached.
    /// </summary>
    Task<bool> PingAsync(CancellationToken ct);

    /// <summary>
    /// "Closed"/"Open"/"HalfOpen"/"Isolated" if this client has a circuit breaker, null if it
    /// doesn't. Read-only — AiKernel surfaces this in telemetry, it never controls the breaker.
    /// </summary>
    string? CircuitState { get; }
}
