using System.Net;
using System.Text;
using AiStudyOS.Infrastructure.AI.Providers;
using FluentAssertions;

namespace AiStudyOS.Infrastructure.UnitTests.AI.Providers;

public class OllamaChatClientTests
{
    // Captured verbatim from a real local Ollama (llama3.1) response — Ollama's wire format is
    // lowercase ("model", "message", "done"); OllamaChatResponseDto's C# properties are PascalCase.
    // If anyone removes the explicit JsonPropertyName mapping or the shared AiJsonOptions.Default
    // from OllamaChatClient, these exact regression tests fail.
    private const string RealCompleteResponseBody =
        """{"model":"llama3.1","created_at":"2026-07-20T10:27:17.8235756Z","message":{"role":"assistant","content":"hello"},"done":true,"done_reason":"stop","total_duration":2376901700,"load_duration":192692500,"prompt_eval_count":16,"prompt_eval_duration":464829000,"eval_count":32,"eval_duration":1651798000}""";

    private const string RealStreamingNdjson =
        """
        {"model":"llama3.1","created_at":"2026-07-20T10:27:17.6Z","message":{"role":"assistant","content":"{ "},"done":false}
        {"model":"llama3.1","created_at":"2026-07-20T10:27:17.7Z","message":{"role":"assistant","content":"}"},"done":false}
        {"model":"llama3.1","created_at":"2026-07-20T10:27:17.8Z","message":{"role":"assistant","content":""},"done":true,"done_reason":"stop","total_duration":2376901700,"load_duration":192692500,"prompt_eval_count":16,"prompt_eval_duration":464829000,"eval_count":32,"eval_duration":1651798000}

        """;

    private static AiChatRequest SampleRequest(bool jsonMode = false) =>
        new([new AiChatMessage("user", "say hello")], "llama3.1", JsonMode: jsonMode);

    private static OllamaChatClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder,
        TimeSpan? timeout = null)
    {
        var handler = new FakeHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://fake-ollama.test"),
            Timeout = timeout ?? TimeSpan.FromSeconds(30),
        };
        // A fresh circuit breaker per client: none of these tests fire 3 consecutive failures
        // through the same client, so it stays Closed and is transparent to every test here.
        // Circuit-breaker-specific behavior is covered in OllamaCircuitBreakerTests.
        return new OllamaChatClient(httpClient, new OllamaCircuitBreaker("ollama"));
    }

    private static Task<HttpResponseMessage> Ok(string body, string contentType = "application/json") =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, contentType) });

    // ---------------------------------------------------------------------------------------
    // Regression tests for the exact bug: lowercase Ollama wire format vs. case-sensitive parsing
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task CompleteAsync_ParsesRealLowercaseOllamaResponse_RegressionTest()
    {
        var client = CreateClient((_, _) => Ok(RealCompleteResponseBody));

        var result = await client.CompleteAsync(SampleRequest(), CancellationToken.None);

        result.Content.Should().Be("hello");
        result.ModelUsed.Should().Be("llama3.1");
        result.PromptTokens.Should().Be(16);
        result.CompletionTokens.Should().Be(32);
    }

    [Fact]
    public async Task StreamAsync_ParsesRealLowercaseNdjson_RegressionTest()
    {
        var client = CreateClient((_, _) => Ok(RealStreamingNdjson, "application/x-ndjson"));

        var deltas = new List<string>();
        await foreach (var delta in client.StreamAsync(SampleRequest(jsonMode: true), CancellationToken.None))
            deltas.Add(delta);

        string.Concat(deltas).Should().Be("{ }");
    }

    // ---------------------------------------------------------------------------------------
    // Defensive parsing / contract validation
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task CompleteAsync_MalformedJsonBody_ThrowsProviderProtocolException()
    {
        var client = CreateClient((_, _) => Ok("this is not json"));

        var act = () => client.CompleteAsync(SampleRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<ProviderProtocolException>();
    }

    [Fact]
    public async Task StreamAsync_MalformedJsonLine_ThrowsProviderProtocolException()
    {
        var client = CreateClient((_, _) => Ok("not-json-at-all\n", "application/x-ndjson"));

        var act = async () =>
        {
            await foreach (var _ in client.StreamAsync(SampleRequest(), CancellationToken.None)) { }
        };

        await act.Should().ThrowAsync<ProviderProtocolException>();
    }

    [Fact]
    public async Task CompleteAsync_MissingMessageField_ThrowsProviderProtocolException()
    {
        var client = CreateClient((_, _) => Ok("""{"model":"llama3.1","done":true}"""));

        var act = () => client.CompleteAsync(SampleRequest(), CancellationToken.None);

        (await act.Should().ThrowAsync<ProviderProtocolException>()).Which.Message.Should().Contain("message");
    }

    [Fact]
    public async Task StreamAsync_MissingMessageField_ThrowsProviderProtocolException()
    {
        var client = CreateClient((_, _) => Ok("""{"model":"llama3.1","done":false}""" + "\n", "application/x-ndjson"));

        var act = async () =>
        {
            await foreach (var _ in client.StreamAsync(SampleRequest(), CancellationToken.None)) { }
        };

        (await act.Should().ThrowAsync<ProviderProtocolException>()).Which.Message.Should().Contain("message");
    }

    [Fact]
    public async Task CompleteAsync_MissingModelField_ThrowsProviderProtocolException()
    {
        var client = CreateClient((_, _) => Ok("""{"message":{"role":"assistant","content":"hi"},"done":true}"""));

        var act = () => client.CompleteAsync(SampleRequest(), CancellationToken.None);

        (await act.Should().ThrowAsync<ProviderProtocolException>()).Which.Message.Should().Contain("model");
    }

    [Fact]
    public async Task StreamAsync_UnknownExtraFields_AreIgnored()
    {
        var body = """{"model":"llama3.1","message":{"role":"assistant","content":"hi"},"done":true,"some_future_field":{"nested":true},"another":123}""" + "\n";
        var client = CreateClient((_, _) => Ok(body, "application/x-ndjson"));

        var deltas = new List<string>();
        await foreach (var delta in client.StreamAsync(SampleRequest(), CancellationToken.None))
            deltas.Add(delta);

        string.Concat(deltas).Should().Be("hi");
    }

    [Fact]
    public async Task StreamAsync_BlankLinesBetweenChunks_AreSkipped()
    {
        var body = "\n\n" +
                   """{"model":"llama3.1","message":{"role":"assistant","content":"a"},"done":false}""" + "\n\n" +
                   """{"model":"llama3.1","message":{"role":"assistant","content":"b"},"done":true}""" + "\n";
        var client = CreateClient((_, _) => Ok(body, "application/x-ndjson"));

        var deltas = new List<string>();
        await foreach (var delta in client.StreamAsync(SampleRequest(), CancellationToken.None))
            deltas.Add(delta);

        string.Concat(deltas).Should().Be("ab");
    }

    [Fact]
    public async Task StreamAsync_StopsAtDoneTrue_IgnoresTrailingLines()
    {
        var body = """{"model":"llama3.1","message":{"role":"assistant","content":"a"},"done":true}""" + "\n" +
                   """{"model":"llama3.1","message":{"role":"assistant","content":"should-not-appear"},"done":true}""" + "\n";
        var client = CreateClient((_, _) => Ok(body, "application/x-ndjson"));

        var deltas = new List<string>();
        await foreach (var delta in client.StreamAsync(SampleRequest(), CancellationToken.None))
            deltas.Add(delta);

        string.Concat(deltas).Should().Be("a");
    }

    // ---------------------------------------------------------------------------------------
    // Cancellation, timeout, disconnect
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task StreamAsync_CancelledMidStream_PropagatesCancellation()
    {
        var ndjson = """{"model":"llama3.1","message":{"role":"assistant","content":"a"},"done":false}""" + "\n" +
                     """{"model":"llama3.1","message":{"role":"assistant","content":"b"},"done":false}""" + "\n" +
                     """{"model":"llama3.1","message":{"role":"assistant","content":""},"done":true}""" + "\n";
        var bytes = Encoding.UTF8.GetBytes(ndjson);

        var client = CreateClient((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new SlowStream(bytes, TimeSpan.FromMilliseconds(200))),
        }));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));

        var act = async () =>
        {
            await foreach (var _ in client.StreamAsync(SampleRequest(), cts.Token)) { }
        };

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CompleteAsync_ProviderTimesOut_ThrowsTaskCanceledException()
    {
        var client = CreateClient(async (_, ct) =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        }, timeout: TimeSpan.FromMilliseconds(50));

        var act = () => client.CompleteAsync(SampleRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task CompleteAsync_ProviderDisconnects_PropagatesHttpRequestException()
    {
        var client = CreateClient((_, _) => throw new HttpRequestException("Connection reset by peer"));

        var act = () => client.CompleteAsync(SampleRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task StreamAsync_ProviderDisconnectsMidStream_ThrowsIOException()
    {
        var ndjson = """{"model":"llama3.1","message":{"role":"assistant","content":"a"},"done":false}""" + "\n" +
                     """{"model":"llama3.1","message":{"role":"assistant","content":"b"},"done":false}""" + "\n";
        var bytes = Encoding.UTF8.GetBytes(ndjson);

        var client = CreateClient((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new DisconnectingStream(bytes, bytesBeforeDisconnect: 20)),
        }));

        var act = async () =>
        {
            await foreach (var _ in client.StreamAsync(SampleRequest(), CancellationToken.None)) { }
        };

        await act.Should().ThrowAsync<IOException>();
    }

    // ---------------------------------------------------------------------------------------
    // Streaming == non-streaming parity
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task CompleteAsync_And_StreamAsync_ProduceIdenticalContent_ForEquivalentPayloads()
    {
        const string expectedContent = "The quick brown fox";

        var completeBody = $$"""{"model":"llama3.1","message":{"role":"assistant","content":"{{expectedContent}}"},"done":true,"prompt_eval_count":5,"eval_count":4}""";
        var streamBody =
            """{"model":"llama3.1","message":{"role":"assistant","content":"The quick "},"done":false}""" + "\n" +
            """{"model":"llama3.1","message":{"role":"assistant","content":"brown fox"},"done":false}""" + "\n" +
            """{"model":"llama3.1","message":{"role":"assistant","content":""},"done":true,"prompt_eval_count":5,"eval_count":4}""" + "\n";

        var completeClient = CreateClient((_, _) => Ok(completeBody));
        var streamClient = CreateClient((_, _) => Ok(streamBody, "application/x-ndjson"));

        var completeResult = await completeClient.CompleteAsync(SampleRequest(), CancellationToken.None);

        var deltas = new List<string>();
        await foreach (var delta in streamClient.StreamAsync(SampleRequest(), CancellationToken.None))
            deltas.Add(delta);
        var streamedContent = string.Concat(deltas);

        completeResult.Content.Should().Be(expectedContent);
        streamedContent.Should().Be(expectedContent);
        streamedContent.Should().Be(completeResult.Content);
    }
}
