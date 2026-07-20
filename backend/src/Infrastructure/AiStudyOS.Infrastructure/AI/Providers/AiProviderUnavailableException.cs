namespace AiStudyOS.Infrastructure.AI.Providers;

/// <summary>
/// Thrown instead of calling the provider at all when that provider's circuit breaker is open — a
/// run of consecutive failures already proved the provider is unreachable, so this fails instantly
/// rather than waiting on a request that's overwhelmingly likely to fail too. Distinct from
/// ProviderProtocolException (a single bad response) and HttpRequestException (a single failed
/// call): this means "we didn't even try."
/// </summary>
public class AiProviderUnavailableException : Exception
{
    public string ProviderKey { get; }

    public AiProviderUnavailableException(string providerKey, string message, Exception? innerException = null)
        : base($"[{providerKey}] {message}", innerException)
    {
        ProviderKey = providerKey;
    }
}
