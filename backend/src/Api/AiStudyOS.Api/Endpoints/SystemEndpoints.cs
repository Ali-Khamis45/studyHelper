using AiStudyOS.Application.Diagnostics.Queries.GetAgentMetrics;
using AiStudyOS.Application.Diagnostics.Queries.GetAiHealth;
using Mediator;

namespace AiStudyOS.Api.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/system").WithTags("System");

        group.MapGet("/ai", async (IMediator mediator, CancellationToken ct) =>
        {
            var health = await mediator.Send(new GetAiHealthQuery(), ct);

            object response = health.IsHealthy
                ? new { provider = health.Provider, model = health.Model, status = "healthy", latencyMs = health.LatencyMs }
                : new { provider = health.Provider, status = "offline", message = health.Message };

            return Results.Ok(response);
        });

        group.MapGet("/agents", async (IMediator mediator, CancellationToken ct) =>
        {
            var metrics = await mediator.Send(new GetAgentMetricsQuery(), ct);
            return Results.Ok(metrics);
        });

        return app;
    }
}
