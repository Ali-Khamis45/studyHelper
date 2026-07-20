namespace AiStudyOS.Application.Common.Options;

/// <summary>
/// AiKernel's retry policy, the provider circuit breaker, and the health-check cache — all
/// validated at startup (see DependencyInjection.AddInfrastructure's ValidateOnStart()), so an
/// invalid value fails the app immediately rather than surfacing as confusing runtime behavior.
/// </summary>
public class AiResilienceOptions
{
    public const string SectionName = "AiResilience";

    public int MaxRetryAttempts { get; init; } = 2;
    public int CircuitBreakerFailureThreshold { get; init; } = 3;
    public int CircuitBreakerBreakDurationSeconds { get; init; } = 30;
    public int HealthCacheDurationSeconds { get; init; } = 10;
}
