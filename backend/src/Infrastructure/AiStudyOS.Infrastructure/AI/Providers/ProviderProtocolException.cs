namespace AiStudyOS.Infrastructure.AI.Providers;

/// <summary>
/// Thrown when a provider's response violates its own protocol — malformed JSON, a missing required
/// field, an unexpected shape. Distinguishes "the provider sent something we can't trust" from
/// network failures (HttpRequestException/TaskCanceledException, already retried by AiKernel) and
/// from our own agent output not matching its schema (JsonException, handled separately as
/// AiKernel's "JsonParseFailed" case). Provider adapters raise this from their own validation —
/// AiKernel and everything above it only ever sees a validated provider response.
/// </summary>
public class ProviderProtocolException : Exception
{
    public string ProviderKey { get; }

    public ProviderProtocolException(string providerKey, string message, Exception? innerException = null)
        : base($"[{providerKey}] {message}", innerException)
    {
        ProviderKey = providerKey;
    }
}
