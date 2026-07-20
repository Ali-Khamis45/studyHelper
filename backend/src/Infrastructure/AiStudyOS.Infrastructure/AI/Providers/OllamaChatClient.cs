using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using AiStudyOS.Infrastructure.Common.Json;

namespace AiStudyOS.Infrastructure.AI.Providers;

public class OllamaChatClient(HttpClient httpClient, OllamaCircuitBreaker circuitBreaker) : IAiChatClient
{
    public string ProviderKey => "ollama";

    public string? CircuitState => circuitBreaker.State;

    public async Task<bool> PingAsync(CancellationToken ct)
    {
        try
        {
            using var response = await circuitBreaker.ExecuteAsync(innerCt => httpClient.GetAsync("/api/tags", innerCt), ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or AiProviderUnavailableException)
        {
            return false;
        }
    }

    public Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct)
    {
        var payload = BuildPayload(request, stream: false);

        return circuitBreaker.ExecuteAsync(async innerCt =>
        {
            using var response = await httpClient.PostAsJsonAsync("/api/chat", payload, AiJsonOptions.Default, innerCt);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync(innerCt);
            var body = ProviderResponseParser.DeserializeResponse<OllamaChatResponseDto>(ProviderKey, raw);
            ValidateChunk(body);

            return new AiChatResponse(
                Content: body.Message!.Content,
                PromptTokens: body.PromptEvalCount ?? 0,
                CompletionTokens: body.EvalCount ?? 0,
                ModelUsed: body.Model);
        }, ct);
    }

    public async IAsyncEnumerable<string> StreamAsync(AiChatRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        var payload = BuildPayload(request, stream: true);

        // The connection-establishment phase AND every subsequent line read both go through the
        // circuit breaker — a mid-stream disconnect (IOException from ReadLineAsync, e.g. the
        // provider closing the connection after headers were already sent) is just as real a
        // provider failure as a connection refusal, and must count the same way. Retrying on that
        // failure is still AiKernel's job — StreamAsync just needs to make sure the breaker sees it.
        var response = await circuitBreaker.ExecuteAsync(async innerCt =>
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
            {
                Content = JsonContent.Create(payload, options: AiJsonOptions.Default),
            };
            var resp = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, innerCt);
            resp.EnsureSuccessStatusCode();
            return resp;
        }, ct);

        using (response)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (true)
            {
                var line = await circuitBreaker.ExecuteAsync(innerCt => reader.ReadLineAsync(innerCt).AsTask(), ct);
                if (line is null) break; // end of stream

                var chunk = ProviderResponseParser.DeserializeStreamingChunk<OllamaChatResponseDto>(ProviderKey, line);
                if (chunk is null) continue; // blank line — transport noise, not a chunk

                ValidateChunk(chunk);

                if (!string.IsNullOrEmpty(chunk.Message!.Content))
                    yield return chunk.Message.Content;

                if (chunk.Done) yield break;
            }
        }
    }

    /// <summary>
    /// The provider contract validation both CompleteAsync and StreamAsync go through — a malformed
    /// chunk (missing model or message) never reaches the caller as a null-reference crash, it always
    /// surfaces as a structured ProviderProtocolException.
    /// </summary>
    private static void ValidateChunk(OllamaChatResponseDto chunk)
    {
        if (string.IsNullOrEmpty(chunk.Model))
            throw new ProviderProtocolException("ollama", "Response is missing the required 'model' field.");

        if (chunk.Message is null)
            throw new ProviderProtocolException("ollama", "Response is missing the required 'message' field.");
    }

    private static OllamaChatRequestDto BuildPayload(AiChatRequest request, bool stream) => new(
        Model: request.Model,
        Messages: request.Messages.Select(m => new OllamaMessageDto(m.Role, m.Content)).ToArray(),
        Stream: stream,
        Format: request.JsonMode ? "json" : null,
        Options: new OllamaOptionsDto(request.Temperature));

    private record OllamaChatRequestDto(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<OllamaMessageDto> Messages,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("format")] string? Format,
        [property: JsonPropertyName("options")] OllamaOptionsDto Options);

    private record OllamaMessageDto(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record OllamaOptionsDto(
        [property: JsonPropertyName("temperature")] double Temperature);

    private record OllamaChatResponseDto(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("message")] OllamaMessageDto? Message,
        [property: JsonPropertyName("done")] bool Done,
        [property: JsonPropertyName("prompt_eval_count")] int? PromptEvalCount,
        [property: JsonPropertyName("eval_count")] int? EvalCount);
}
