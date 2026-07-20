using System.Runtime.CompilerServices;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Infrastructure.AI.Providers;
using AiStudyOS.Infrastructure.AI.Telemetry;
using AiStudyOS.Infrastructure.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiStudyOS.Infrastructure.UnitTests.AI.Kernel;

public class AiKernelCancellationTests
{
    private record TestPayload(int A, int B);

    internal sealed class RecordingTelemetryRecorder : IAiTelemetryRecorder
    {
        private readonly List<AiTelemetryRecord> _records = [];
        public IReadOnlyList<AiTelemetryRecord> Records => _records;

        public Task RecordAsync(AiTelemetryRecord record, CancellationToken ct)
        {
            lock (_records) _records.Add(record);
            return Task.CompletedTask;
        }
    }

    private static AiStudyOS.Infrastructure.AI.Kernel.AiKernel CreateKernel(IAiChatClient chatClient, IAiTelemetryRecorder telemetry) =>
        new(chatClient, "fake-model", telemetry, new CorrelationIdProvider(), new NoOpAiMetrics(), NullLogger<AiStudyOS.Infrastructure.AI.Kernel.AiKernel>.Instance);

    /// <summary>Streams a fixed set of deltas with a real delay between each, so a test can cancel mid-stream.</summary>
    private sealed class SlowStreamingChatClient : IAiChatClient
    {
        public string ProviderKey => "fake";
        public string? CircuitState => null;
        public int StreamCallCount { get; private set; }

        public Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct) => throw new NotSupportedException();

        public async IAsyncEnumerable<string> StreamAsync(AiChatRequest request, [EnumeratorCancellation] CancellationToken ct)
        {
            StreamCallCount++;
            string[] deltas = ["""{"a":1""", ""","b":2""", "}"];
            foreach (var delta in deltas)
            {
                await Task.Delay(100, ct);
                yield return delta;
            }
        }

        public Task<bool> PingAsync(CancellationToken ct) => Task.FromResult(true);
    }

    private sealed class SlowCompletingChatClient : IAiChatClient
    {
        public string ProviderKey => "fake";
        public string? CircuitState => null;
        public int CompleteCallCount { get; private set; }

        public async Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct)
        {
            CompleteCallCount++;
            await Task.Delay(500, ct);
            return new AiChatResponse("""{"a":1,"b":2}""", 1, 1, "fake-model");
        }

        public IAsyncEnumerable<string> StreamAsync(AiChatRequest request, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> PingAsync(CancellationToken ct) => Task.FromResult(true);
    }

    /// <summary>Fails its first call with a provider-side timeout (not caller cancellation), succeeds on retry.</summary>
    private sealed class TimingOutThenSucceedingChatClient : IAiChatClient
    {
        public string ProviderKey => "fake";
        public string? CircuitState => null;
        private int _attempt;

        public Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct)
        {
            _attempt++;
            if (_attempt == 1)
                throw new TaskCanceledException("simulated provider-side timeout (HttpClient.Timeout), not caller cancellation");

            return Task.FromResult(new AiChatResponse("""{"a":1,"b":2}""", 3, 3, "fake-model"));
        }

        public IAsyncEnumerable<string> StreamAsync(AiChatRequest request, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> PingAsync(CancellationToken ct) => Task.FromResult(true);
    }

    private static KernelRequest BuildRequest() => new(
        AgentType.Recommendation,
        new PromptDefinition(AgentType.Recommendation, "v1", "test", [], """{"a":"number","b":"number"}""", [], "system prompt"),
        new AiContext([], 0),
        ExpectedSchemaJson: """{"a":"number","b":"number"}""");

    [Fact]
    public async Task ExecuteStreamAsync_CancelledMidStream_StopsImmediately_NoRetry_RecordsCancelledTelemetry()
    {
        var telemetry = new RecordingTelemetryRecorder();
        var chatClient = new SlowStreamingChatClient();
        var kernel = CreateKernel(chatClient, telemetry);

        using var cts = new CancellationTokenSource();
        var receivedDeltas = new List<string>();

        var act = async () =>
        {
            await foreach (var chunk in kernel.ExecuteStreamAsync<TestPayload>(BuildRequest(), cts.Token))
            {
                if (chunk.IsFinal) continue;

                receivedDeltas.Add(chunk.DeltaContent);
                if (receivedDeltas.Count == 1)
                    await cts.CancelAsync();
            }
        };

        await act.Should().ThrowAsync<OperationCanceledException>();

        chatClient.StreamCallCount.Should().Be(1, "a cancelled request must not be retried");
        receivedDeltas.Should().HaveCount(1, "streaming must stop as soon as cancellation is observed, not run to completion");

        telemetry.Records.Should().ContainSingle();
        var record = telemetry.Records.Single();
        record.Success.Should().BeFalse();
        record.ErrorType.Should().Be("Cancelled");
        record.CancellationReason.Should().NotBeNullOrEmpty();
        record.Stream.Should().BeTrue();
        record.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledDuringProviderCall_StopsImmediately_NoRetry_RecordsCancelledTelemetry()
    {
        var telemetry = new RecordingTelemetryRecorder();
        var chatClient = new SlowCompletingChatClient();
        var kernel = CreateKernel(chatClient, telemetry);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var act = () => kernel.ExecuteAsync<TestPayload>(BuildRequest(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();

        chatClient.CompleteCallCount.Should().Be(1, "a cancelled request must not be retried");

        telemetry.Records.Should().ContainSingle();
        var record = telemetry.Records.Single();
        record.Success.Should().BeFalse();
        record.ErrorType.Should().Be("Cancelled");
        record.Stream.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ProviderTimeout_NotCallerCancellation_StillRetriesAndSucceeds()
    {
        var telemetry = new RecordingTelemetryRecorder();
        var chatClient = new TimingOutThenSucceedingChatClient();
        var kernel = CreateKernel(chatClient, telemetry);

        var result = await kernel.ExecuteAsync<TestPayload>(BuildRequest(), CancellationToken.None);

        result.Success.Should().BeTrue("a provider-side timeout (not caller cancellation) must still be retried");
        result.Data.Should().BeEquivalentTo(new TestPayload(1, 2));

        telemetry.Records.Should().ContainSingle();
        telemetry.Records.Single().RetryCount.Should().Be(1);
        telemetry.Records.Single().ErrorType.Should().BeNull();
    }
}
