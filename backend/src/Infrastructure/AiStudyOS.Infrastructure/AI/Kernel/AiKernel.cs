using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Infrastructure.AI.Providers;
using AiStudyOS.Infrastructure.Common.Json;
using Microsoft.Extensions.Logging;

namespace AiStudyOS.Infrastructure.AI.Kernel;

/// <summary>
/// Real implementation: resolves the provider, renders the prompt against the merged context,
/// retries on transient failures, repairs/re-prompts on malformed JSON, and records telemetry on
/// every attempt (success, failure, or cancellation). ExecuteAsync and ExecuteStreamAsync share
/// every step of that pipeline — parsing (TryParse), retry policy (MaxAttempts + backoff + repair
/// re-prompt), and telemetry recording — the only difference is that ExecuteStreamAsync also yields
/// provider text as it arrives. Currently backed by a single provider (Ollama) — see
/// DependencyInjection for where a multi-provider resolver would slot in.
///
/// Cancellation is not a failure: if the caller's own CancellationToken is what triggered an
/// OperationCanceledException, this stops immediately, records one "Cancelled" telemetry row (never
/// logged as a warning — see LoggingAiTelemetryRecorder), and rethrows without retrying. A
/// provider-side timeout (HttpClient's own internal token firing) is a different case — that's a
/// real provider failure and goes through the normal retry path.
///
/// Telemetry is best-effort: it is always attempted with CancellationToken.None (never the caller's
/// token, so it survives cancellation) and a persistence failure is logged at Debug and otherwise
/// swallowed — it must never change the outcome of the request that triggered it.
///
/// ActivitySource spans and IAiMetrics calls are OpenTelemetry/metrics *seams*: harmless no-ops
/// until something actually subscribes (an ActivitySource with no listeners costs ~nothing; the
/// current IAiMetrics implementation is a no-op) — see DependencyInjection.
/// </summary>
public class AiKernel(
    IAiChatClient chatClient,
    string defaultModel,
    IAiTelemetryRecorder telemetryRecorder,
    ICorrelationIdProvider correlationIdProvider,
    IAiMetrics metrics,
    ILogger<AiKernel> logger,
    int maxAttempts = 2) : IAiKernel
{
    private static readonly ActivitySource ActivitySource = new("AiStudyOS.AI");

    // Transient, retryable provider failures: a network problem (including a mid-response
    // disconnect, IOException), the provider violating its own protocol (ProviderProtocolException),
    // or HttpClient's own internal timeout firing (a TaskCanceledException whose CancellationToken
    // is NOT the caller's ct). AiProviderUnavailableException (circuit open) and genuine caller
    // cancellation are deliberately excluded — both are handled as their own, non-retried paths.
    private static bool IsRetryableProviderFailure(Exception ex, CancellationToken ct) => ex switch
    {
        HttpRequestException or IOException or ProviderProtocolException => true,
        TaskCanceledException => !ct.IsCancellationRequested,
        _ => false,
    };

    public async Task<KernelResult<T>> ExecuteAsync<T>(KernelRequest request, CancellationToken ct)
    {
        if (request.ExpectedSchemaJson is null && typeof(T) != typeof(string))
            throw new InvalidOperationException($"{nameof(request.ExpectedSchemaJson)} is required unless T is string.");

        var correlationId = correlationIdProvider.CorrelationId;
        var model = request.ModelOverride ?? defaultModel;
        var stopwatch = Stopwatch.StartNew();

        using var activity = StartActivity(request, correlationId, model, stream: false);

        var messages = new List<AiChatMessage> { new("system", RenderTemplate(request.Prompt.Template, request.Context)) };

        var retryCount = 0;
        var jsonRepairCount = 0;
        var errors = new List<string>();
        var lastRawContent = string.Empty;

        try
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                AiChatResponse response;
                try
                {
                    response = await chatClient.CompleteAsync(
                        new AiChatRequest(messages, model, JsonMode: request.ExpectedSchemaJson is not null),
                        ct);
                }
                catch (AiProviderUnavailableException ex)
                {
                    // Circuit is open — fail immediately, no retry, no backoff.
                    var brokenTelemetry = await RecordTelemetryAsync(correlationId, request, model, null, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: false, nameof(AiProviderUnavailableException), stream: false, responseSizeBytes: null);
                    activity?.SetStatus(ActivityStatusCode.Error, nameof(AiProviderUnavailableException));
                    return new KernelResult<T>(default, false, lastRawContent, brokenTelemetry, [ex.Message]);
                }
                catch (Exception ex) when (IsRetryableProviderFailure(ex, ct))
                {
                    logger.LogWarning(ex, "AiKernel transient failure on attempt {Attempt} for {AgentType} via {ProviderKey} [{CorrelationId}]", attempt, request.AgentType, chatClient.ProviderKey, correlationId);
                    errors.Add(ex.Message);

                    if (attempt >= maxAttempts)
                    {
                        var failedTelemetry = await RecordTelemetryAsync(correlationId, request, model, null, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: false, ex.GetType().Name, stream: false, responseSizeBytes: null);
                        activity?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
                        return new KernelResult<T>(default, false, lastRawContent, failedTelemetry, errors);
                    }

                    retryCount++;
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), ct);
                    continue;
                }

                lastRawContent = response.Content;
                var responseSizeBytes = (long)Encoding.UTF8.GetByteCount(response.Content);

                if (request.ExpectedSchemaJson is null)
                {
                    var okTelemetry = await RecordTelemetryAsync(correlationId, request, model, response, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: true, null, stream: false, responseSizeBytes);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return new KernelResult<T>((T)(object)response.Content, true, response.Content, okTelemetry, []);
                }

                var (parsed, parseError) = TryParse<T>(response.Content);
                if (parsed is not null)
                {
                    var okTelemetry = await RecordTelemetryAsync(correlationId, request, model, response, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: true, null, stream: false, responseSizeBytes);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return new KernelResult<T>(parsed, true, response.Content, okTelemetry, []);
                }

                errors.Add(parseError!);
                jsonRepairCount++;
                logger.LogWarning("AiKernel JSON parse failed on attempt {Attempt} for {AgentType} via {ProviderKey} [{CorrelationId}]: {Error}", attempt, request.AgentType, chatClient.ProviderKey, correlationId, parseError);

                if (attempt < maxAttempts)
                    AppendRepairPrompt(messages, response.Content, parseError!);
            }

            var exhaustedTelemetry = await RecordTelemetryAsync(correlationId, request, model, null, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: false, "JsonParseFailed", stream: false, responseSizeBytes: null);
            activity?.SetStatus(ActivityStatusCode.Error, "JsonParseFailed");
            return new KernelResult<T>(default, false, lastRawContent, exhaustedTelemetry, errors);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            var size = lastRawContent.Length == 0 ? (long?)null : Encoding.UTF8.GetByteCount(lastRawContent);
            await RecordCancelledTelemetryAsync(correlationId, request, model, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, stream: false, size);
            activity?.SetStatus(ActivityStatusCode.Error, "Cancelled");
            throw;
        }
    }

    public async IAsyncEnumerable<KernelStreamChunk<T>> ExecuteStreamAsync<T>(KernelRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        if (request.ExpectedSchemaJson is null && typeof(T) != typeof(string))
            throw new InvalidOperationException($"{nameof(request.ExpectedSchemaJson)} is required unless T is string.");

        var correlationId = correlationIdProvider.CorrelationId;
        var model = request.ModelOverride ?? defaultModel;
        var stopwatch = Stopwatch.StartNew();

        using var activity = StartActivity(request, correlationId, model, stream: true);

        var messages = new List<AiChatMessage> { new("system", RenderTemplate(request.Prompt.Template, request.Context)) };

        var retryCount = 0;
        var jsonRepairCount = 0;
        var errors = new List<string>();
        var lastRawContent = string.Empty;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var buffer = new StringBuilder();
            var chatRequest = new AiChatRequest(messages, model, JsonMode: request.ExpectedSchemaJson is not null);
            var enumerator = chatClient.StreamAsync(chatRequest, ct).GetAsyncEnumerator(ct);

            string? transientErrorType = null;
            AiProviderUnavailableException? providerUnavailable = null;
            OperationCanceledException? cancelledException = null;

            try
            {
                while (true)
                {
                    string delta;
                    try
                    {
                        if (!await enumerator.MoveNextAsync()) break;
                        delta = enumerator.Current;
                    }
                    catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
                    {
                        cancelledException = ex;
                        break;
                    }
                    catch (AiProviderUnavailableException ex)
                    {
                        providerUnavailable = ex;
                        break;
                    }
                    catch (Exception ex) when (IsRetryableProviderFailure(ex, ct))
                    {
                        logger.LogWarning(ex, "AiKernel stream transient failure on attempt {Attempt} for {AgentType} via {ProviderKey} [{CorrelationId}]", attempt, request.AgentType, chatClient.ProviderKey, correlationId);
                        transientErrorType = ex.GetType().Name;
                        errors.Add(ex.Message);
                        break;
                    }

                    buffer.Append(delta);
                    yield return new KernelStreamChunk<T>(delta, false);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            lastRawContent = buffer.ToString();
            var responseSizeBytes = lastRawContent.Length == 0 ? (long?)null : Encoding.UTF8.GetByteCount(lastRawContent);

            if (cancelledException is not null)
            {
                await RecordCancelledTelemetryAsync(correlationId, request, model, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, stream: true, responseSizeBytes);
                activity?.SetStatus(ActivityStatusCode.Error, "Cancelled");
                ExceptionDispatchInfo.Capture(cancelledException).Throw();
            }

            if (providerUnavailable is not null)
            {
                var brokenTelemetry = await RecordTelemetryAsync(correlationId, request, model, null, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: false, nameof(AiProviderUnavailableException), stream: true, responseSizeBytes);
                activity?.SetStatus(ActivityStatusCode.Error, nameof(AiProviderUnavailableException));
                yield return new KernelStreamChunk<T>(string.Empty, true, new KernelResult<T>(default, false, lastRawContent, brokenTelemetry, [providerUnavailable.Message]));
                yield break;
            }

            if (transientErrorType is not null)
            {
                if (attempt >= maxAttempts)
                {
                    var failedTelemetry = await RecordTelemetryAsync(correlationId, request, model, null, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: false, transientErrorType, stream: true, responseSizeBytes);
                    activity?.SetStatus(ActivityStatusCode.Error, transientErrorType);
                    yield return new KernelStreamChunk<T>(string.Empty, true, new KernelResult<T>(default, false, lastRawContent, failedTelemetry, errors));
                    yield break;
                }

                retryCount++;
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    await RecordCancelledTelemetryAsync(correlationId, request, model, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, stream: true, responseSizeBytes);
                    activity?.SetStatus(ActivityStatusCode.Error, "Cancelled");
                    throw;
                }

                continue;
            }

            if (request.ExpectedSchemaJson is null)
            {
                var okTelemetry = await RecordTelemetryAsync(correlationId, request, model, null, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: true, null, stream: true, responseSizeBytes);
                activity?.SetStatus(ActivityStatusCode.Ok);
                yield return new KernelStreamChunk<T>(string.Empty, true, new KernelResult<T>((T)(object)lastRawContent, true, lastRawContent, okTelemetry, []));
                yield break;
            }

            var (parsed, parseError) = TryParse<T>(lastRawContent);
            if (parsed is not null)
            {
                var okTelemetry = await RecordTelemetryAsync(correlationId, request, model, null, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: true, null, stream: true, responseSizeBytes);
                activity?.SetStatus(ActivityStatusCode.Ok);
                yield return new KernelStreamChunk<T>(string.Empty, true, new KernelResult<T>(parsed, true, lastRawContent, okTelemetry, []));
                yield break;
            }

            errors.Add(parseError!);
            jsonRepairCount++;
            logger.LogWarning("AiKernel stream JSON parse failed on attempt {Attempt} for {AgentType} via {ProviderKey} [{CorrelationId}]: {Error}", attempt, request.AgentType, chatClient.ProviderKey, correlationId, parseError);

            if (attempt < maxAttempts)
                AppendRepairPrompt(messages, lastRawContent, parseError!);
        }

        var exhaustedTelemetry = await RecordTelemetryAsync(correlationId, request, model, null, stopwatch.ElapsedMilliseconds, retryCount, jsonRepairCount, success: false, "JsonParseFailed", stream: true, responseSizeBytes: null);
        activity?.SetStatus(ActivityStatusCode.Error, "JsonParseFailed");
        yield return new KernelStreamChunk<T>(string.Empty, true, new KernelResult<T>(default, false, lastRawContent, exhaustedTelemetry, errors));
    }

    public async Task<AiHealthResult> CheckHealthAsync(CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var healthy = await chatClient.PingAsync(ct);
            stopwatch.Stop();

            return healthy
                ? new AiHealthResult(chatClient.ProviderKey, defaultModel, true, stopwatch.ElapsedMilliseconds, null)
                : new AiHealthResult(chatClient.ProviderKey, defaultModel, false, stopwatch.ElapsedMilliseconds, $"Unable to connect to {chatClient.ProviderKey}.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogWarning(ex, "AiKernel health check failed for {ProviderKey} [{CorrelationId}]", chatClient.ProviderKey, correlationIdProvider.CorrelationId);
            return new AiHealthResult(chatClient.ProviderKey, defaultModel, false, stopwatch.ElapsedMilliseconds, $"Unable to connect to {chatClient.ProviderKey}.");
        }
    }

    private static Activity? StartActivity(KernelRequest request, string correlationId, string model, bool stream)
    {
        var activity = ActivitySource.StartActivity("ai.kernel.execute", ActivityKind.Client);
        activity?.SetTag("ai.correlation_id", correlationId);
        activity?.SetTag("ai.agent_type", request.AgentType.ToString());
        activity?.SetTag("ai.model", model);
        activity?.SetTag("ai.stream", stream);
        return activity;
    }

    private async Task<AiTelemetryRecord> RecordTelemetryAsync(
        string correlationId, KernelRequest request, string model, AiChatResponse? response,
        long latencyMs, int retryCount, int jsonRepairCount, bool success, string? errorType,
        bool stream, long? responseSizeBytes)
    {
        var record = new AiTelemetryRecord(
            correlationId,
            request.AgentType,
            chatClient.ProviderKey,
            model,
            request.Prompt.Version,
            response?.PromptTokens ?? 0,
            response?.CompletionTokens ?? 0,
            EstimatedCostUsd: 0m, // local Ollama inference has no per-token cost
            latencyMs,
            retryCount,
            jsonRepairCount,
            ToolCallCount: 0,
            success,
            errorType,
            DateTime.UtcNow,
            Stream: stream,
            Cached: false, // a kernel execution only ever happens when the caller's own result cache missed
            CircuitBreakerState: chatClient.CircuitState,
            ResponseSizeBytes: responseSizeBytes,
            CancellationReason: null);

        await PersistTelemetryAsync(record);
        RecordMetrics(record);
        return record;
    }

    private async Task<AiTelemetryRecord> RecordCancelledTelemetryAsync(
        string correlationId, KernelRequest request, string model, long latencyMs, int retryCount, int jsonRepairCount,
        bool stream, long? responseSizeBytes)
    {
        var record = new AiTelemetryRecord(
            correlationId,
            request.AgentType,
            chatClient.ProviderKey,
            model,
            request.Prompt.Version,
            PromptTokens: 0,
            CompletionTokens: 0,
            EstimatedCostUsd: 0m,
            latencyMs,
            retryCount,
            jsonRepairCount,
            ToolCallCount: 0,
            Success: false,
            ErrorType: "Cancelled",
            DateTime.UtcNow,
            Stream: stream,
            Cached: false,
            CircuitBreakerState: chatClient.CircuitState,
            ResponseSizeBytes: responseSizeBytes,
            CancellationReason: "The request was cancelled by the caller (client disconnected or the operation was explicitly cancelled).");

        await PersistTelemetryAsync(record);
        RecordMetrics(record);
        return record;
    }

    /// <summary>
    /// Telemetry is best-effort: always attempted, never on the caller's own (possibly already
    /// cancelled) token, and a persistence failure here must never change the request's outcome —
    /// it's logged at Debug and swallowed, not rethrown.
    /// </summary>
    private async Task PersistTelemetryAsync(AiTelemetryRecord record)
    {
        try
        {
            await telemetryRecorder.RecordAsync(record, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "AiKernel telemetry persistence failed for {CorrelationId} — request result is unaffected.", record.CorrelationId);
        }
    }

    private void RecordMetrics(AiTelemetryRecord record)
    {
        try
        {
            metrics.ObserveLatency(record.ProviderKey, record.AgentType, record.Stream, record.LatencyMs);

            if (record.ErrorType == "Cancelled")
                metrics.IncrementCancellation(record.ProviderKey, record.AgentType);
            else if (record.Success)
                metrics.IncrementGeneration(record.ProviderKey, record.AgentType, record.Stream);
            else
                metrics.IncrementFailure(record.ProviderKey, record.AgentType, record.ErrorType ?? "Unknown");
        }
        catch (Exception ex)
        {
            // Same guarantee as telemetry: a metrics backend problem must never affect the request.
            logger.LogDebug(ex, "AiKernel metrics recording failed for {CorrelationId}.", record.CorrelationId);
        }
    }

    private static void AppendRepairPrompt(List<AiChatMessage> messages, string previousContent, string parseError)
    {
        messages.Add(new AiChatMessage("assistant", previousContent));
        messages.Add(new AiChatMessage(
            "user",
            $"Your previous response was not valid JSON: {parseError}. Return ONLY the corrected JSON object — no commentary, no markdown code fences."));
    }

    private static string RenderTemplate(string template, AiContext context)
    {
        var contextText = string.Join(
            "\n\n",
            context.Fragments.OrderByDescending(f => f.Priority).Select(f => $"## {f.SectionName}\n{f.Content}"));

        return template.Replace("{{context}}", contextText);
    }

    private static (T? Value, string? Error) TryParse<T>(string raw)
    {
        var cleaned = StripCodeFences(raw);
        try
        {
            var value = JsonSerializer.Deserialize<T>(cleaned, AiJsonOptions.Default);
            return value is null ? (default, "Deserialized to null.") : (value, null);
        }
        catch (JsonException ex)
        {
            return (default, ex.Message);
        }
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```")) return trimmed;

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline >= 0) trimmed = trimmed[(firstNewline + 1)..];
        if (trimmed.EndsWith("```")) trimmed = trimmed[..^3];

        return trimmed.Trim();
    }
}
