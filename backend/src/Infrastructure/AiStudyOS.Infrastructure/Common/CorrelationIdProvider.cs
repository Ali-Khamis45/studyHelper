using System.Diagnostics;
using AiStudyOS.Application.Common.Interfaces;

namespace AiStudyOS.Infrastructure.Common;

/// <summary>
/// Captures Activity.Current?.Id (or generates a new id) exactly once, at construction — since this
/// is registered scoped, that happens on first resolution within a request and every subsequent
/// resolution in the same scope returns the same instance with the same value.
/// </summary>
public class CorrelationIdProvider : ICorrelationIdProvider
{
    public string CorrelationId { get; } = Activity.Current?.Id ?? Guid.NewGuid().ToString();
}
