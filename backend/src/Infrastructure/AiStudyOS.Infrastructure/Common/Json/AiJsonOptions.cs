using System.Text.Json;

namespace AiStudyOS.Infrastructure.Common.Json;

/// <summary>
/// The single JsonSerializerOptions instance for every JSON operation in this project that crosses
/// a boundary this project doesn't control — AI provider wire formats (request and response), and
/// the model's own structured output. Every JsonSerializer.Deserialize/Serialize, ReadFromJsonAsync,
/// and JsonContent.Create call must pass this explicitly; none may rely on a method's implicit
/// default (which differs between System.Net.Http.Json helpers and bare JsonSerializer calls — that
/// mismatch is exactly what caused a NullReferenceException in the Ollama streaming client).
///
/// Case-insensitive as a defensive fallback only. DTOs should still declare explicit
/// [JsonPropertyName] wherever the wire format doesn't already match .NET naming, so correctness
/// never actually depends on this policy.
/// </summary>
public static class AiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
}
