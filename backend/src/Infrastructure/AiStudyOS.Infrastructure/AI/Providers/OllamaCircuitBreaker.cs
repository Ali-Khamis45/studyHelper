using Polly;
using Polly.CircuitBreaker;

namespace AiStudyOS.Infrastructure.AI.Providers;

/// <summary>
/// Wraps every real network call OllamaChatClient makes (CompleteAsync, StreamAsync's connection
/// phase, PingAsync) in one shared Polly circuit breaker, so a run of provider failures fails fast
/// instead of continuing to retry into an offline provider. Owned entirely by the provider layer —
/// AiKernel and everything above it never touches Polly directly, they only ever see
/// AiProviderUnavailableException when the circuit is open.
///
/// Registered as a singleton (see DependencyInjection) so its state persists across requests —
/// OllamaChatClient itself is created per-resolution, but always shares this one breaker.
///
/// Polly's ExecuteAsync respects the CancellationToken passed to each call independently of the
/// exception types below: if the caller's own token is what triggered a TaskCanceledException, Polly
/// treats it as a genuine cancellation (not a handled fault) and neither retries nor counts it
/// against the circuit, regardless of the Or&lt;TaskCanceledException&gt;() registration — that
/// registration only matters for a provider-side timeout (HttpClient.Timeout firing on its own
/// internal token), which should count as a real provider failure.
/// </summary>
public class OllamaCircuitBreaker
{
    private const int DefaultFailuresBeforeBreaking = 3;
    private static readonly TimeSpan DefaultBreakDuration = TimeSpan.FromSeconds(30);

    private readonly AsyncCircuitBreakerPolicy _policy;
    private readonly string _providerKey;
    private readonly int _failuresBeforeBreaking;
    private readonly TimeSpan _breakDuration;

    /// <param name="failuresBeforeBreaking">Defaults to 3, matching spec. Overridable for tests only.</param>
    /// <param name="breakDuration">Defaults to 30s, matching spec. Overridable for tests only.</param>
    public OllamaCircuitBreaker(string providerKey, int failuresBeforeBreaking = DefaultFailuresBeforeBreaking, TimeSpan? breakDuration = null)
    {
        _providerKey = providerKey;
        _failuresBeforeBreaking = failuresBeforeBreaking;
        _breakDuration = breakDuration ?? DefaultBreakDuration;
        _policy = Policy
            .Handle<HttpRequestException>()
            .Or<IOException>()
            .Or<TaskCanceledException>()
            .Or<ProviderProtocolException>()
            .CircuitBreakerAsync(_failuresBeforeBreaking, _breakDuration);
    }

    /// <summary>"Closed", "Open", "HalfOpen", or "Isolated" — for telemetry only.</summary>
    public string State => _policy.CircuitState.ToString();

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        try
        {
            return await _policy.ExecuteAsync(action, ct);
        }
        catch (BrokenCircuitException ex)
        {
            throw new AiProviderUnavailableException(
                _providerKey,
                $"The circuit breaker is open — {_providerKey} has failed {_failuresBeforeBreaking} times in a row and is being given {_breakDuration.TotalSeconds}s to recover.",
                ex);
        }
    }
}
