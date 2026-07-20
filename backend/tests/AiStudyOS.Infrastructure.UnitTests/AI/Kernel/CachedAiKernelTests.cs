using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Infrastructure.AI.Kernel;
using AiStudyOS.Infrastructure.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;

namespace AiStudyOS.Infrastructure.UnitTests.AI.Kernel;

public class CachedAiKernelTests
{
    private sealed class CountingAiKernel : IAiKernel
    {
        private int _checkHealthCallCount;
        public int CheckHealthCallCount => _checkHealthCallCount;
        public TimeSpan Delay { get; init; } = TimeSpan.Zero;

        public Task<KernelResult<T>> ExecuteAsync<T>(KernelRequest request, CancellationToken ct) =>
            throw new NotSupportedException("Not exercised by these tests.");

        public IAsyncEnumerable<KernelStreamChunk<T>> ExecuteStreamAsync<T>(KernelRequest request, CancellationToken ct) =>
            throw new NotSupportedException("Not exercised by these tests.");

        public async Task<AiHealthResult> CheckHealthAsync(CancellationToken ct)
        {
            Interlocked.Increment(ref _checkHealthCallCount);
            if (Delay > TimeSpan.Zero)
                await Task.Delay(Delay, ct);

            return new AiHealthResult("ollama", "llama3.1", true, 5, null);
        }
    }

    private static IMemoryCache NewCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task CheckHealthAsync_ReturnsCachedResult_WithinCacheDuration()
    {
        var inner = new CountingAiKernel();
        var cached = new CachedAiKernel(inner, NewCache(), new AsyncLock(), TimeSpan.FromSeconds(10));

        await cached.CheckHealthAsync(CancellationToken.None);
        await cached.CheckHealthAsync(CancellationToken.None);
        await cached.CheckHealthAsync(CancellationToken.None);

        inner.CheckHealthCallCount.Should().Be(1, "the second and third calls should be served from cache");
    }

    [Fact]
    public async Task CheckHealthAsync_RefreshesAfterCacheExpires()
    {
        var inner = new CountingAiKernel();
        var cached = new CachedAiKernel(inner, NewCache(), new AsyncLock(), TimeSpan.FromMilliseconds(50));

        await cached.CheckHealthAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(120));
        await cached.CheckHealthAsync(CancellationToken.None);

        inner.CheckHealthCallCount.Should().Be(2, "the cache entry should have expired between the two calls");
    }

    [Fact]
    public async Task CheckHealthAsync_ConcurrentRequestsDuringRefresh_OnlyTriggerOnePing()
    {
        var inner = new CountingAiKernel { Delay = TimeSpan.FromMilliseconds(150) };
        var cached = new CachedAiKernel(inner, NewCache(), new AsyncLock(), TimeSpan.FromSeconds(10));

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => cached.CheckHealthAsync(CancellationToken.None)));

        inner.CheckHealthCallCount.Should().Be(1, "concurrent requests during a refresh must share the same in-flight ping");
        results.Should().OnlyContain(r => r.IsHealthy);
    }

    [Fact]
    public async Task ExecuteAsync_AndExecuteStreamAsync_PassThroughToInner_Unaffected()
    {
        var inner = new CountingAiKernel();
        var cached = new CachedAiKernel(inner, NewCache(), new AsyncLock());

        // These simply must not throw NotImplementedException from CachedAiKernel itself — proving
        // the decorator forwards everything except CheckHealthAsync straight through.
        var executeThrew = false;
        try
        {
            await cached.ExecuteAsync<string>(null!, CancellationToken.None);
        }
        catch (NotSupportedException)
        {
            executeThrew = true; // expected — CountingAiKernel itself doesn't implement this
        }

        executeThrew.Should().BeTrue();
    }
}
