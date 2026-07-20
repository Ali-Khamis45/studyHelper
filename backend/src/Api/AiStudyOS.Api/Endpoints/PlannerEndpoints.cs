using AiStudyOS.Application.Planner.Commands.CompleteTask;
using AiStudyOS.Application.Planner.Commands.GenerateDailyRecommendation;
using AiStudyOS.Application.Planner.Commands.RescheduleTask;
using AiStudyOS.Application.Planner.Commands.SkipTask;
using AiStudyOS.Application.Planner.Queries.GetToday;
using AiStudyOS.Application.Planner.Queries.GetWeek;
using AiStudyOS.Application.Planner.Streaming;
using Mediator;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AiStudyOS.Api.Endpoints;

public record RescheduleTaskRequest(DateOnly NewDate);

public static class PlannerEndpoints
{
    public static IEndpointRouteBuilder MapPlannerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/planner").WithTags("Planner");

        group.MapGet("/today", async (IMediator mediator, CancellationToken ct) =>
        {
            var plan = await mediator.Send(new GetTodayQuery(), ct);
            return Results.Ok(plan);
        });

        group.MapPost("/recommendations/generate", async (IMediator mediator, CancellationToken ct) =>
        {
            var plan = await mediator.Send(new GenerateDailyRecommendationCommand(), ct);
            return Results.Ok(plan);
        });

        group.MapPost("/recommendations/stream", async (HttpContext httpContext, IRecommendationStreamer streamer, IOptions<JsonOptions> jsonOptions, CancellationToken ct) =>
        {
            httpContext.Response.ContentType = "application/x-ndjson";
            httpContext.Response.Headers.CacheControl = "no-cache";

            var serializerOptions = jsonOptions.Value.SerializerOptions;

            await foreach (var streamEvent in streamer.StreamTodayRecommendationAsync(ct))
            {
                object payload = streamEvent switch
                {
                    RecommendationDeltaEvent delta => new { type = "delta", content = delta.Content },
                    RecommendationCompleteEvent complete => new { type = "complete", plan = complete.Plan },
                    RecommendationErrorEvent error => new { type = "error", message = error.Message },
                    _ => new { type = "unknown" },
                };

                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload, serializerOptions), ct);
                await httpContext.Response.WriteAsync("\n", ct);
                await httpContext.Response.Body.FlushAsync(ct);
            }
        });

        group.MapGet("/week", async (IMediator mediator, CancellationToken ct) =>
        {
            var week = await mediator.Send(new GetWeekQuery(), ct);
            return Results.Ok(week);
        });

        group.MapPatch("/tasks/{id:guid}/complete", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new CompleteTaskCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPatch("/tasks/{id:guid}/skip", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new SkipTaskCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPatch("/tasks/{id:guid}/reschedule", async (Guid id, RescheduleTaskRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new RescheduleTaskCommand(id, request.NewDate), ct);
            return Results.NoContent();
        });

        return app;
    }
}
