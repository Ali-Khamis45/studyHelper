using System.Text.Json;
using AiStudyOS.Application.Quiz.Commands.DeleteQuiz;
using AiStudyOS.Application.Quiz.Commands.GenerateQuiz;
using AiStudyOS.Application.Quiz.Commands.RetryQuiz;
using AiStudyOS.Application.Quiz.Commands.SubmitQuiz;
using AiStudyOS.Application.Quiz.Queries.GetAttempt;
using AiStudyOS.Application.Quiz.Queries.GetQuiz;
using AiStudyOS.Application.Quiz.Queries.GetQuizHistory;
using AiStudyOS.Application.Quiz.Queries.GetQuizzes;
using AiStudyOS.Application.Quiz.Queries.GetTopicMastery;
using AiStudyOS.Application.Quiz.Queries.GetWeakTopics;
using AiStudyOS.Application.Quiz.Streaming;
using AiStudyOS.Domain.Quiz;
using Mediator;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Api.Endpoints;

public record GenerateQuizRequest(string? Topic, Guid? GoalId, Difficulty Difficulty, IReadOnlyList<QuestionType> QuestionTypes, int QuestionCount, QuizType QuizType);
public record SubmitQuizRequest(Guid QuizId, IReadOnlyList<SubmittedAnswer> Answers);

public static class QuizEndpoints
{
    public static IEndpointRouteBuilder MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/quiz").WithTags("Quiz");

        group.MapGet("/", async (int? page, int? pageSize, IMediator mediator, CancellationToken ct) =>
        {
            var quizzes = await mediator.Send(new GetQuizzesQuery(page ?? 1, pageSize ?? 0), ct);
            return Results.Ok(quizzes);
        });

        group.MapPost("/generate", async (GenerateQuizRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var quiz = await mediator.Send(new GenerateQuizCommand(request.Topic, request.GoalId, request.Difficulty, request.QuestionTypes, request.QuestionCount, request.QuizType), ct);
            return Results.Created($"/api/v1/quiz/{quiz.Id}", quiz);
        });

        group.MapPost("/generate/stream", async (GenerateQuizRequest request, HttpContext httpContext, IQuizGenerationStreamer streamer, IOptions<JsonOptions> jsonOptions, CancellationToken ct) =>
        {
            httpContext.Response.ContentType = "application/x-ndjson";
            httpContext.Response.Headers.CacheControl = "no-cache";

            var serializerOptions = jsonOptions.Value.SerializerOptions;

            await foreach (var streamEvent in streamer.StreamGenerateAsync(request.Topic, request.GoalId, request.Difficulty, request.QuestionTypes, request.QuestionCount, request.QuizType, ct))
            {
                object payload = streamEvent switch
                {
                    QuizGenerationDeltaEvent delta => new { type = "delta", content = delta.Content },
                    QuizGenerationCompleteEvent complete => new { type = "complete", quiz = complete.Quiz },
                    QuizGenerationErrorEvent error => new { type = "error", message = error.Message },
                    _ => new { type = "unknown" },
                };

                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload, serializerOptions), ct);
                await httpContext.Response.WriteAsync("\n", ct);
                await httpContext.Response.Body.FlushAsync(ct);
            }
        });

        group.MapPost("/submit", async (SubmitQuizRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SubmitQuizCommand(request.QuizId, request.Answers), ct);
            return Results.Ok(result);
        });

        group.MapGet("/history", async (int? page, int? pageSize, IMediator mediator, CancellationToken ct) =>
        {
            var history = await mediator.Send(new GetQuizHistoryQuery(page ?? 1, pageSize ?? 0), ct);
            return Results.Ok(history);
        });

        group.MapGet("/attempts/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var attempt = await mediator.Send(new GetAttemptQuery(id), ct);
            return Results.Ok(attempt);
        });

        group.MapGet("/mastery", async (IMediator mediator, CancellationToken ct) =>
        {
            var mastery = await mediator.Send(new GetTopicMasteryQuery(), ct);
            return Results.Ok(mastery);
        });

        group.MapGet("/weak-topics", async (int? take, IMediator mediator, CancellationToken ct) =>
        {
            var weakTopics = await mediator.Send(new GetWeakTopicsQuery(take), ct);
            return Results.Ok(weakTopics);
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var quiz = await mediator.Send(new GetQuizQuery(id), ct);
            return Results.Ok(quiz);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteQuizCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapPost("/{id:guid}/retry", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var quiz = await mediator.Send(new RetryQuizCommand(id), ct);
            return Results.Ok(quiz);
        });

        return app;
    }
}
