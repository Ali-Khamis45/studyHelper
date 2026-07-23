using System.Text.Json;
using AiStudyOS.Application.Mentor.Commands.CreateConversation;
using AiStudyOS.Application.Mentor.Commands.DeleteConversation;
using AiStudyOS.Application.Mentor.Commands.RenameConversation;
using AiStudyOS.Application.Mentor.Commands.SendMessage;
using AiStudyOS.Application.Mentor.Commands.SetConversationPinned;
using AiStudyOS.Application.Mentor.Queries.GetConversation;
using AiStudyOS.Application.Mentor.Queries.GetConversationMessages;
using AiStudyOS.Application.Mentor.Queries.GetConversations;
using AiStudyOS.Application.Mentor.Streaming;
using Mediator;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Api.Endpoints;

public record CreateConversationRequest(string? Title);
public record RenameConversationRequest(string Title);
public record SetConversationPinnedRequest(bool IsPinned);
public record SendMessageRequest(string Content);

public static class MentorEndpoints
{
    public static IEndpointRouteBuilder MapMentorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/mentor").WithTags("Mentor");

        group.MapGet("/conversations", async (string? search, bool? pinned, int? page, int? pageSize, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetConversationsQuery(search, pinned, page ?? 1, pageSize ?? 0), ct);
            return Results.Ok(result);
        });

        group.MapPost("/conversations", async (CreateConversationRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var conversation = await mediator.Send(new CreateConversationCommand(request.Title), ct);
            return Results.Created($"/api/v1/mentor/conversations/{conversation.Id}", conversation);
        });

        group.MapGet("/conversations/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var conversation = await mediator.Send(new GetConversationQuery(id), ct);
            return Results.Ok(conversation);
        });

        group.MapPatch("/conversations/{id:guid}/rename", async (Guid id, RenameConversationRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var conversation = await mediator.Send(new RenameConversationCommand(id, request.Title), ct);
            return Results.Ok(conversation);
        });

        group.MapPatch("/conversations/{id:guid}/pin", async (Guid id, SetConversationPinnedRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var conversation = await mediator.Send(new SetConversationPinnedCommand(id, request.IsPinned), ct);
            return Results.Ok(conversation);
        });

        group.MapDelete("/conversations/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteConversationCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapGet("/conversations/{id:guid}/messages", async (Guid id, int? page, int? pageSize, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetConversationMessagesQuery(id, page ?? 1, pageSize ?? 0), ct);
            return Results.Ok(result);
        });

        group.MapPost("/conversations/{id:guid}/messages", async (Guid id, SendMessageRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var message = await mediator.Send(new SendMessageCommand(id, request.Content), ct);
            return Results.Ok(message);
        });

        group.MapPost("/conversations/{id:guid}/messages/stream", async (Guid id, SendMessageRequest request, HttpContext httpContext, IMentorMessageStreamer streamer, IOptions<JsonOptions> jsonOptions, CancellationToken ct) =>
        {
            httpContext.Response.ContentType = "application/x-ndjson";
            httpContext.Response.Headers.CacheControl = "no-cache";

            var serializerOptions = jsonOptions.Value.SerializerOptions;

            await foreach (var streamEvent in streamer.StreamMessageAsync(id, request.Content, ct))
            {
                object payload = streamEvent switch
                {
                    MentorDeltaEvent delta => new { type = "delta", content = delta.Content },
                    MentorCompleteEvent complete => new { type = "complete", message = complete.Message },
                    MentorErrorEvent error => new { type = "error", message = error.Message },
                    _ => new { type = "unknown" },
                };

                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload, serializerOptions), ct);
                await httpContext.Response.WriteAsync("\n", ct);
                await httpContext.Response.Body.FlushAsync(ct);
            }
        });

        return app;
    }
}
