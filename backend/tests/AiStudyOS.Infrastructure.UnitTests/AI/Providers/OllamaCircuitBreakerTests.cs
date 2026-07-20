using AiStudyOS.Infrastructure.AI.Providers;
using FluentAssertions;

namespace AiStudyOS.Infrastructure.UnitTests.AI.Providers;

public class OllamaCircuitBreakerTests
{
    private static Task<string> Failing(CancellationToken _) => throw new HttpRequestException("connection refused");
    private static Task<string> Succeeding(CancellationToken _) => Task.FromResult("ok");

    [Fact]
    public async Task Circuit_OpensAfterThreeConsecutiveFailures()
    {
        var breaker = new OllamaCircuitBreaker("ollama", failuresBeforeBreaking: 3, breakDuration: TimeSpan.FromSeconds(30));

        for (var i = 0; i < 3; i++)
        {
            var act = () => breaker.ExecuteAsync(Failing, CancellationToken.None);
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        breaker.State.Should().Be("Open");
    }

    [Fact]
    public async Task Circuit_StaysClosedIfAFailureIsNotConsecutive()
    {
        var breaker = new OllamaCircuitBreaker("ollama", failuresBeforeBreaking: 3, breakDuration: TimeSpan.FromSeconds(30));

        await Catch(() => breaker.ExecuteAsync(Failing, CancellationToken.None));
        await Catch(() => breaker.ExecuteAsync(Failing, CancellationToken.None));
        await breaker.ExecuteAsync(Succeeding, CancellationToken.None); // resets the consecutive-failure count
        await Catch(() => breaker.ExecuteAsync(Failing, CancellationToken.None));

        breaker.State.Should().Be("Closed");
    }

    [Fact]
    public async Task Circuit_SkipsProviderCall_WhileOpen()
    {
        var breaker = new OllamaCircuitBreaker("ollama", failuresBeforeBreaking: 3, breakDuration: TimeSpan.FromSeconds(30));
        for (var i = 0; i < 3; i++)
            await Catch(() => breaker.ExecuteAsync(Failing, CancellationToken.None));

        breaker.State.Should().Be("Open");

        var callCount = 0;
        Task<string> CountingAction(CancellationToken _)
        {
            callCount++;
            return Task.FromResult("should not run");
        }

        var act = () => breaker.ExecuteAsync(CountingAction, CancellationToken.None);

        await act.Should().ThrowAsync<AiProviderUnavailableException>();
        callCount.Should().Be(0, "the circuit is open — the provider must not be called at all");
    }

    [Fact]
    public async Task Circuit_HalfOpen_ClosesOnSuccessfulTrial()
    {
        var breaker = new OllamaCircuitBreaker("ollama", failuresBeforeBreaking: 3, breakDuration: TimeSpan.FromMilliseconds(100));
        for (var i = 0; i < 3; i++)
            await Catch(() => breaker.ExecuteAsync(Failing, CancellationToken.None));

        breaker.State.Should().Be("Open");

        await Task.Delay(TimeSpan.FromMilliseconds(150));

        var result = await breaker.ExecuteAsync(Succeeding, CancellationToken.None);

        result.Should().Be("ok");
        breaker.State.Should().Be("Closed");
    }

    [Fact]
    public async Task Circuit_HalfOpen_ReOpensOnFailedTrial()
    {
        var breaker = new OllamaCircuitBreaker("ollama", failuresBeforeBreaking: 3, breakDuration: TimeSpan.FromMilliseconds(100));
        for (var i = 0; i < 3; i++)
            await Catch(() => breaker.ExecuteAsync(Failing, CancellationToken.None));

        breaker.State.Should().Be("Open");

        await Task.Delay(TimeSpan.FromMilliseconds(150));

        await Catch(() => breaker.ExecuteAsync(Failing, CancellationToken.None));

        breaker.State.Should().Be("Open");
    }

    [Fact]
    public async Task Circuit_DoesNotCountGenuineCallerCancellation_TowardFailures()
    {
        var breaker = new OllamaCircuitBreaker("ollama", failuresBeforeBreaking: 3, breakDuration: TimeSpan.FromSeconds(30));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Task<string> Cancellable(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult("unreachable");
        }

        for (var i = 0; i < 5; i++)
        {
            var act = () => breaker.ExecuteAsync(Cancellable, cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        breaker.State.Should().Be("Closed", "caller-initiated cancellation is not a provider failure");
    }

    private static async Task Catch(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch
        {
            // expected — these calls exist purely to drive the breaker's failure count
        }
    }
}
