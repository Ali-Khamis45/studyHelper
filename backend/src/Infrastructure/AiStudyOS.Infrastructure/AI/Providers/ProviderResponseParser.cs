using System.Text.Json;
using AiStudyOS.Infrastructure.Common.Json;

namespace AiStudyOS.Infrastructure.AI.Providers;

/// <summary>
/// Shared parsing entry point for every IAiChatClient implementation. Both the complete-response and
/// streaming-chunk code paths of a provider adapter must deserialize through here, so they always
/// use the same JsonSerializerOptions and always fail the same way — a structured
/// ProviderProtocolException, never an unhandled JsonException or a downstream NullReferenceException
/// from an unchecked null.
/// </summary>
public static class ProviderResponseParser
{
    /// <summary>Parses a single complete JSON response body (CompleteAsync).</summary>
    public static T DeserializeResponse<T>(string providerKey, string json) where T : class
    {
        T? value;
        try
        {
            value = JsonSerializer.Deserialize<T>(json, AiJsonOptions.Default);
        }
        catch (JsonException ex)
        {
            throw new ProviderProtocolException(providerKey, $"Response body is not valid JSON: {ex.Message}", ex);
        }

        return value ?? throw new ProviderProtocolException(providerKey, "Response body deserialized to null.");
    }

    /// <summary>
    /// Parses one line of an NDJSON streaming response (StreamAsync). Returns null only for
    /// whitespace/empty input (transport noise, not a chunk) — any non-empty line that fails to
    /// parse throws, it is never silently skipped.
    /// </summary>
    public static T? DeserializeStreamingChunk<T>(string providerKey, string? line) where T : class
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        try
        {
            return JsonSerializer.Deserialize<T>(line, AiJsonOptions.Default);
        }
        catch (JsonException ex)
        {
            throw new ProviderProtocolException(providerKey, $"Streaming chunk is not valid JSON: {ex.Message}", ex);
        }
    }
}
