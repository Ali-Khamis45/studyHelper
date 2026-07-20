namespace AiStudyOS.Application.Common.Interfaces;

/// <summary>
/// One correlation identifier per request/scope — prefers the ambient System.Diagnostics
/// Activity.Current.Id (present whenever any tracing listener, e.g. OpenTelemetry, is active) and
/// falls back to a freshly generated id otherwise. Registered scoped, so every layer (endpoint,
/// kernel, provider, telemetry, logs) that reads it during the same request sees the identical value.
/// </summary>
public interface ICorrelationIdProvider
{
    string CorrelationId { get; }
}
