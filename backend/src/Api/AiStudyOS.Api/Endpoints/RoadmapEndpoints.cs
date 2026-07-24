using System.Text.Json;
using AiStudyOS.Application.Roadmap.Ai;
using AiStudyOS.Application.Roadmap.Commands.CompleteTopic;
using AiStudyOS.Application.Roadmap.Commands.DeleteRoadmap;
using AiStudyOS.Application.Roadmap.Commands.GenerateRoadmap;
using AiStudyOS.Application.Roadmap.Queries.GetRoadmap;
using AiStudyOS.Application.Roadmap.Queries.GetRoadmaps;
using AiStudyOS.Application.Roadmap.Streaming;
using Mediator;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Api.Endpoints;

public record GenerateRoadmapRequest(
    string CareerGoal,
    string? CurrentExperience,
    string? ExistingSkills,
    int? HoursPerWeek,
    string? LearningStyle,
    DateOnly? TargetCompletionDate,
    string? PreferredLanguage,
    string? PreferredResources);

public record CompleteTopicRequest(bool Completed);

public static class RoadmapEndpoints
{
    public static IEndpointRouteBuilder MapRoadmapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/roadmaps").WithTags("Roadmap");

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var roadmaps = await mediator.Send(new GetRoadmapsQuery(), ct);
            return Results.Ok(roadmaps);
        });

        group.MapPost("/generate", async (GenerateRoadmapRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var roadmap = await mediator.Send(ToCommand(request), ct);
            return Results.Created($"/api/v1/roadmaps/{roadmap.Id}", roadmap);
        });

        group.MapPost("/generate/stream", async (GenerateRoadmapRequest request, HttpContext httpContext, IRoadmapGenerationStreamer streamer, IOptions<JsonOptions> jsonOptions, CancellationToken ct) =>
        {
            httpContext.Response.ContentType = "application/x-ndjson";
            httpContext.Response.Headers.CacheControl = "no-cache";

            var serializerOptions = jsonOptions.Value.SerializerOptions;
            var profile = ToProfile(request);

            await foreach (var streamEvent in streamer.StreamGenerateAsync(profile, ct))
            {
                object payload = streamEvent switch
                {
                    RoadmapGenerationDeltaEvent delta => new { type = "delta", content = delta.Content },
                    RoadmapGenerationCompleteEvent complete => new { type = "complete", roadmap = complete.Roadmap },
                    RoadmapGenerationErrorEvent error => new { type = "error", message = error.Message },
                    _ => new { type = "unknown" },
                };

                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload, serializerOptions), ct);
                await httpContext.Response.WriteAsync("\n", ct);
                await httpContext.Response.Body.FlushAsync(ct);
            }
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var roadmap = await mediator.Send(new GetRoadmapQuery(id), ct);
            return Results.Ok(roadmap);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteRoadmapCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapPatch("/{id:guid}/topics/{topicId:guid}/complete", async (Guid id, Guid topicId, CompleteTopicRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var updated = await mediator.Send(new CompleteTopicCommand(id, topicId, request.Completed), ct);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private static GenerateRoadmapCommand ToCommand(GenerateRoadmapRequest r) => new(
        r.CareerGoal, r.CurrentExperience, r.ExistingSkills, r.HoursPerWeek, r.LearningStyle, r.TargetCompletionDate, r.PreferredLanguage, r.PreferredResources);

    private static RoadmapProfile ToProfile(GenerateRoadmapRequest r) => new(
        r.CareerGoal, r.CurrentExperience, r.ExistingSkills, r.HoursPerWeek, r.LearningStyle, r.TargetCompletionDate, r.PreferredLanguage, r.PreferredResources);
}
