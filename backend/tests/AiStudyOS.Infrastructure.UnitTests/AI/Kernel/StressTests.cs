using System.Runtime.CompilerServices;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Infrastructure.AI.Kernel;
using AiStudyOS.Infrastructure.AI.Providers;
using AiStudyOS.Infrastructure.AI.Telemetry;
using AiStudyOS.Infrastructure.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiStudyOS.Infrastructure.UnitTests.AI.Kernel;

/// <summary>
/// Concurrency correctness, not performance: every test here bounds itself with WaitAsync so a
/// genuine deadlock fails the test (TimeoutException) instead of hanging the test run forever.
/// </summary>
public class StressTests
{
    private record TestPayload(int A);

    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(20);

    private static KernelRequest BuildRequest() => new(
        AgentType.Recommendation,
        new PromptDefinition(AgentType.Recommendation, "v1", "test", [], """{"a":"number"}""", [], "system prompt"),
        new AiContext([], 0),
        ExpectedSchemaJson: """{"a":"number"}""");

    private static AiStudyOS.Infrastructure.AI.Kernel.AiKernel CreateKernel(IAiChatClient chatClient, IAiTelemetryRecorder telemetry) =>
        new(chatClient, "fake-model", telemetry, new CorrelationIdProvider(), new NoOpAiMetrics(), NullLogger<AiStudyOS.Infrastructure.AI.Kernel.AiKernel>.Instance);

    private sealed class FastSucceedingChatClient : IAiChatClient
    {
        public string ProviderKey => "fake";
        public string? CircuitState => null;

        public Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct) =>
            Task.FromResult(new AiChatResponse("""{"a":1}""", 1, 1, "fake-model"));

        public IAsyncEnumerable<string> StreamAsync(AiChatRequest request, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> PingAsync(CancellationToken ct) => Task.FromResult(true);
    }

    private sealed class CountingHealthKernel : IAiKernel
    {
        private int _callCount;
        public int CallCount => _callCount;

        public Task<KernelResult<T>> ExecuteAsync<T>(KernelRequest request, CancellationToken ct) => throw new NotSupportedException();
        public IAsyncEnumerable<KernelStreamChunk<T>> ExecuteStreamAsync<T>(KernelRequest request, CancellationToken ct) => throw new NotSupportedException();

        public async Task<AiHealthResult> CheckHealthAsync(CancellationToken ct)
        {
            Interlocked.Increment(ref _callCount);
            await Task.Delay(20, ct);
            return new AiHealthResult("ollama", "llama3.1", true, 5, null);
        }
    }

    /// <summary>Streams a couple of small deltas with a short delay, tuned for stress-test speed rather than realism.</summary>
    private sealed class SlowStreamingChatClient : IAiChatClient
    {
        public string ProviderKey => "fake";
        public string? CircuitState => null;

        public Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct) => throw new NotSupportedException();

        public async IAsyncEnumerable<string> StreamAsync(AiChatRequest request, [EnumeratorCancellation] CancellationToken ct)
        {
            for (var i = 0; i < 5; i++)
            {
                await Task.Delay(15, ct);
                yield return i == 4 ? "1}" : """{"a":""";
            }
        }

        public Task<bool> PingAsync(CancellationToken ct) => Task.FromResult(true);
    }

    [Fact]
    public async Task CachedAiKernel_100ConcurrentHealthRequests_NoDeadlock_SingleFlightHolds()
    {
        var inner = new CountingHealthKernel();
        var cached = new CachedAiKernel(inner, new MemoryCache(new MemoryCacheOptions()), new AsyncLock(), TimeSpan.FromSeconds(10));

        var results = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => cached.CheckHealthAsync(CancellationToken.None)))
            .WaitAsync(Deadline);

        results.Should().OnlyContain(r => r.IsHealthy);
        inner.CallCount.Should().BeLessThanOrEqualTo(2, "single-flight must keep real pings very low even under 100-way concurrency");
    }

    [Fact]
    public async Task AiKernel_100ConcurrentGenerationRequests_NoDeadlock_AllSucceed_TelemetryComplete()
    {
        var telemetry = new AiKernelCancellationTests.RecordingTelemetryRecorder();
        var kernel = CreateKernel(new FastSucceedingChatClient(), telemetry);

        var results = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => kernel.ExecuteAsync<TestPayload>(BuildRequest(), CancellationToken.None)))
            .WaitAsync(Deadline);

        results.Should().OnlyContain(r => r.Success);
        telemetry.Records.Should().HaveCount(100, "concurrent telemetry writes must not be lost or duplicated");
    }

    [Fact]
    public async Task CircuitBreaker_RapidUpDownTransitions_NoDeadlock_NoUnexpectedExceptions()
    {
        var breaker = new OllamaCircuitBreaker("ollama", failuresBeforeBreaking: 3, breakDuration: TimeSpan.FromMilliseconds(50));

        var tasks = Enumerable.Range(0, 300).Select(async _ =>
        {
            try
            {
                if (Random.Shared.Next(2) == 0)
                    await breaker.ExecuteAsync<string>(_ => throw new HttpRequestException("simulated down"), CancellationToken.None);
                else
                    await breaker.ExecuteAsync(_ => Task.FromResult("ok"), CancellationToken.None);
            }
            catch (HttpRequestException)
            {
                // expected — half the calls simulate a failing provider
            }
            catch (AiProviderUnavailableException)
            {
                // expected — the circuit may be open when this call lands
            }
        });

        await Task.WhenAll(tasks).WaitAsync(Deadline);

        breaker.State.Should().BeOneOf("Closed", "Open", "HalfOpen");
    }

    [Fact]
    public async Task CircuitBreaker_ConcurrentCalls_StateRemainsConsistent()
    {
        var breaker = new OllamaCircuitBreaker("ollama", failuresBeforeBreaking: 3, breakDuration: TimeSpan.FromSeconds(30));

        // All-failing, concurrently: the breaker must still end up Open (>=3 failures observed),
        // never throw anything other than HttpRequestException/AiProviderUnavailableException, and
        // never deadlock despite every caller hitting the same shared policy at once.
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            try
            {
                await breaker.ExecuteAsync<string>(_ => throw new HttpRequestException("simulated down"), CancellationToken.None);
            }
            catch (HttpRequestException) { }
            catch (AiProviderUnavailableException) { }
        });

        await Task.WhenAll(tasks).WaitAsync(Deadline);

        breaker.State.Should().Be("Open");
    }

    [Fact]
    public async Task AiKernel_CancellationStorm_NoDeadlock_NoUnhandledExceptions_TelemetryAlwaysRecorded()
    {
        var telemetry = new AiKernelCancellationTests.RecordingTelemetryRecorder();
        var kernel = CreateKernel(new SlowStreamingChatClient(), telemetry);

        var tasks = Enumerable.Range(0, 40).Select(async i =>
        {
            using var cts = new CancellationTokenSource();
            if (i % 2 == 0)
                cts.CancelAfter(TimeSpan.FromMilliseconds(Random.Shared.Next(5, 60)));

            try
            {
                await foreach (var chunk in kernel.ExecuteStreamAsync<TestPayload>(BuildRequest(), cts.Token))
                {
                    if (chunk.IsFinal) break;
                }
            }
            catch (OperationCanceledException)
            {
                // expected for the half of requests that get cancelled
            }
        });

        await Task.WhenAll(tasks).WaitAsync(Deadline);

        telemetry.Records.Should().HaveCount(40, "every request — cancelled or completed — must produce exactly one telemetry row");
        telemetry.Records.Where(r => r.ErrorType == "Cancelled").Should().OnlyContain(r => r.RetryCount == 0);
    }
}
