using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Infrastructure.Common;
using Microsoft.Extensions.Caching.Memory;

namespace AiStudyOS.Infrastructure.AI.Kernel;

/// <summary>
/// Decorates the real AiKernel to cache CheckHealthAsync results for 10 seconds — the health
/// endpoint gets polled frequently (the frontend polls it every 30s, and could be hit more often by
/// monitoring), and a real provider ping isn't needed on every call. Every other IAiKernel method
/// passes straight through unchanged; this never touches ExecuteAsync/ExecuteStreamAsync.
///
/// refreshLock is injected as a singleton (see DependencyInjection) — CachedAiKernel itself is
/// scoped (it wraps the scoped AiKernel), so a new instance exists per request, but concurrent
/// requests must still serialize onto the *same* refresh. The underlying IMemoryCache is already a
/// singleton; the lock needs to be shared the same way, without resorting to static state.
/// </summary>
public class CachedAiKernel(IAiKernel inner, IMemoryCache cache, IAsyncLock refreshLock, TimeSpan? cacheDuration = null) : IAiKernel
{
    private const string CacheKey = "ai-health-check:ollama";

    // Defaults to 10s, matching spec. Overridable for tests only — production always uses the default.
    private readonly TimeSpan _cacheDuration = cacheDuration ?? TimeSpan.FromSeconds(10);

    public Task<KernelResult<T>> ExecuteAsync<T>(KernelRequest request, CancellationToken ct) =>
        inner.ExecuteAsync<T>(request, ct);

    public IAsyncEnumerable<KernelStreamChunk<T>> ExecuteStreamAsync<T>(KernelRequest request, CancellationToken ct) =>
        inner.ExecuteStreamAsync<T>(request, ct);

    public async Task<AiHealthResult> CheckHealthAsync(CancellationToken ct)
    {
        if (cache.TryGetValue(CacheKey, out AiHealthResult? cached) && cached is not null)
            return cached;

        using (await refreshLock.AcquireAsync(ct))
        {
            // Re-check: whoever held the lock first may have already refreshed it while we waited.
            if (cache.TryGetValue(CacheKey, out cached) && cached is not null)
                return cached;

            var result = await inner.CheckHealthAsync(ct);
            cache.Set(CacheKey, result, _cacheDuration);
            return result;
        }
    }
}
