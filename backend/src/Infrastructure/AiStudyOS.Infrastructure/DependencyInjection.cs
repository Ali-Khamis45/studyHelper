using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Memory;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Routing;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.AI.Tools;
using AiStudyOS.Application.AI.Tools.Implementations;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Mentor.Ai;
using AiStudyOS.Application.Mentor.Streaming;
using AiStudyOS.Application.Planner.Streaming;
using AiStudyOS.Application.Quiz.Streaming;
using AiStudyOS.Application.Roadmap.Streaming;
using AiStudyOS.Domain.Identity;
using AiStudyOS.Infrastructure.AI.Kernel;
using AiStudyOS.Infrastructure.AI.Memory;
using AiStudyOS.Infrastructure.AI.Prompts;
using AiStudyOS.Infrastructure.AI.Providers;
using AiStudyOS.Infrastructure.AI.Routing;
using AiStudyOS.Infrastructure.AI.Telemetry;
using AiStudyOS.Infrastructure.Common;
using AiStudyOS.Infrastructure.Identity;
using AiStudyOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AccountLockoutOptions>(configuration.GetSection(AccountLockoutOptions.SectionName));
        services.Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName));
        services.Configure<RateLimitOptions>(configuration.GetSection(RateLimitOptions.SectionName));
        services.Configure<PlannerOptions>(configuration.GetSection(PlannerOptions.SectionName));
        services.Configure<MentorOptions>(configuration.GetSection(MentorOptions.SectionName));
        services.Configure<QuizOptions>(configuration.GetSection(QuizOptions.SectionName));
        services.Configure<AnalyticsOptions>(configuration.GetSection(AnalyticsOptions.SectionName));

        // Community license: free for individuals/companies under $1M USD annual gross revenue —
        // appropriate here. Set once at startup; QuestPDF reads this statically wherever it's used.
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // Validated eagerly at startup (ValidateOnStart) rather than lazily on first use — a bad
        // Ollama URL or an impossible circuit-breaker/cache setting fails the app immediately
        // instead of surfacing as confusing runtime behavior the first time AI is actually used.
        services.AddOptions<OllamaOptions>()
            .Bind(configuration.GetSection(OllamaOptions.SectionName))
            .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _), $"{OllamaOptions.SectionName}:{nameof(OllamaOptions.BaseUrl)} must be a valid absolute URI.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.DefaultModel), $"{OllamaOptions.SectionName}:{nameof(OllamaOptions.DefaultModel)} must not be empty.")
            .Validate(o => o.TimeoutSeconds > 0, $"{OllamaOptions.SectionName}:{nameof(OllamaOptions.TimeoutSeconds)} must be positive.")
            .ValidateOnStart();

        services.AddOptions<AiResilienceOptions>()
            .Bind(configuration.GetSection(AiResilienceOptions.SectionName))
            .Validate(o => o.MaxRetryAttempts >= 1, $"{AiResilienceOptions.SectionName}:{nameof(AiResilienceOptions.MaxRetryAttempts)} must be at least 1.")
            .Validate(o => o.CircuitBreakerFailureThreshold >= 1, $"{AiResilienceOptions.SectionName}:{nameof(AiResilienceOptions.CircuitBreakerFailureThreshold)} must be at least 1.")
            .Validate(o => o.CircuitBreakerBreakDurationSeconds >= 1, $"{AiResilienceOptions.SectionName}:{nameof(AiResilienceOptions.CircuitBreakerBreakDurationSeconds)} must be at least 1.")
            .Validate(o => o.HealthCacheDurationSeconds >= 0, $"{AiResilienceOptions.SectionName}:{nameof(AiResilienceOptions.HealthCacheDurationSeconds)} must not be negative.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // --- AI Kernel stack ---------------------------------------------------------------
        // Single provider (Ollama) for now; a resolver keyed by AgentDefinition.AllowedProviders
        // is the natural extension point once a second provider is added.

        // Singleton: the circuit breaker's state (failure count, open/closed) must persist across
        // requests — OllamaChatClient itself stays request-scoped, but always shares this one breaker.
        services.AddSingleton(sp =>
        {
            var resilience = sp.GetRequiredService<IOptions<AiResilienceOptions>>().Value;
            return new OllamaCircuitBreaker(
                "ollama",
                resilience.CircuitBreakerFailureThreshold,
                TimeSpan.FromSeconds(resilience.CircuitBreakerBreakDurationSeconds));
        });

        services.AddHttpClient<IAiChatClient, OllamaChatClient>((sp, client) =>
        {
            var ollama = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
            client.BaseAddress = new Uri(ollama.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(ollama.TimeoutSeconds);
        });

        // Scoped (not singleton) because PostgresAiTelemetryRecorder depends on the scoped
        // IApplicationDbContext; CompositeAiTelemetryRecorder fans out to both the durable
        // (Postgres) and immediate-visibility (log) recorders so telemetry survives restarts (§1).
        services.AddScoped<PostgresAiTelemetryRecorder>();
        services.AddScoped<LoggingAiTelemetryRecorder>();
        services.AddScoped<IAiTelemetryRecorder, CompositeAiTelemetryRecorder>();
        services.AddSingleton<IPromptLibrary, FilePromptLibrary>();

        // No metrics backend exists yet — see IAiMetrics/NoOpAiMetrics. Swapping in a real one
        // later (Prometheus, OTEL Metrics) only means changing this one registration.
        services.AddSingleton<IAiMetrics, NoOpAiMetrics>();

        // One correlation id per request, preferring Activity.Current when a tracing listener is
        // active (see CorrelationIdProvider) — scoped so every consumer within the same request
        // (AiKernel, the response-header middleware) resolves the identical value.
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();

        // Singleton, DI-managed — not static — so CachedAiKernel (scoped) can still coordinate a
        // single in-flight health-check refresh across concurrent requests without static state.
        services.AddSingleton<IAsyncLock, AsyncLock>();

        // IMemoryCache backs CachedAiKernel's health-check cache — its storage is a singleton
        // regardless of the scoped services that read/write it.
        services.AddMemoryCache();

        // CachedAiKernel wraps the real AiKernel to cache CheckHealthAsync only; every other IAiKernel
        // consumer (recommendation generation, streaming) is unaffected and unaware caching exists.
        services.AddScoped<IAiKernel>(sp =>
        {
            var resilience = sp.GetRequiredService<IOptions<AiResilienceOptions>>().Value;

            var kernel = new AiKernel(
                sp.GetRequiredService<IAiChatClient>(),
                sp.GetRequiredService<IOptions<OllamaOptions>>().Value.DefaultModel,
                sp.GetRequiredService<IAiTelemetryRecorder>(),
                sp.GetRequiredService<ICorrelationIdProvider>(),
                sp.GetRequiredService<IAiMetrics>(),
                sp.GetRequiredService<ILogger<AiKernel>>(),
                resilience.MaxRetryAttempts);

            return new CachedAiKernel(
                kernel,
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<IAsyncLock>(),
                TimeSpan.FromSeconds(resilience.HealthCacheDurationSeconds));
        });

        // Context providers are resolved by ContextBuilder via their concrete type (an
        // AgentDefinition.ContextProviders entry), so each must be registered by its own type.
        services.AddScoped<GoalContextProvider>();
        services.AddScoped<TaskContextProvider>();
        services.AddScoped<TimeOfDayContextProvider>();
        services.AddScoped<ConversationContextProvider>();
        services.AddScoped<MemoryContextProvider>();
        services.AddScoped<AnalyticsSnapshotContextProvider>();
        services.AddScoped<QuizRequestContextProvider>();
        services.AddScoped<TopicMasteryContextProvider>();
        services.AddScoped<QuizHistoryContextProvider>();
        services.AddScoped<RoadmapProfileContextProvider>();
        services.AddScoped<IContextBuilder, ContextBuilder>();

        services.AddScoped<ITool, PlannerTool>();
        services.AddScoped<IToolRegistry, ToolRegistry>();
        services.AddScoped<IToolExecutor, ToolExecutor>();
        services.AddScoped<IRecommendationStreamer, RecommendationStreamer>();

        // --- Mentor stack -------------------------------------------------------------------
        services.AddScoped<IMemoryStore, PostgresMemoryStore>();
        services.AddSingleton<IIntentClassifier, KeywordIntentClassifier>();
        services.AddScoped<MentorConversationStore>();
        services.AddScoped<IMentorMessageStreamer, MentorMessageStreamer>();

        // --- Quiz stack ---------------------------------------------------------------------
        services.AddScoped<IQuizGenerationStreamer, QuizGenerationStreamer>();

        // --- Roadmap stack --------------------------------------------------------------------
        services.AddScoped<IRoadmapGenerationStreamer, RoadmapGenerationStreamer>();

        services.AddSingleton<IAgentRegistry>(_ =>
        {
            var registry = new AgentRegistry();
            registry.Register(RecommendationAgentDefinition.Create());
            registry.Register(TutorAgentDefinition.Create());
            registry.Register(PlannerChatAgentDefinition.Create());
            registry.Register(AnalyticsAgentDefinition.Create());
            registry.Register(ExaminerAgentDefinition.Create());
            registry.Register(QuizGeneratorAgentDefinition.Create());
            registry.Register(InsightsAgentDefinition.Create());
            registry.Register(RoadmapGeneratorAgentDefinition.Create());
            registry.Register(RoadmapChatAgentDefinition.Create());
            return registry;
        });

        return services;
    }
}
