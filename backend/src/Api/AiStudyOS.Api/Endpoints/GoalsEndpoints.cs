using AiStudyOS.Application.Goals.Commands.CreateGoal;
using AiStudyOS.Application.Goals.Commands.DeleteGoal;
using AiStudyOS.Application.Goals.Commands.UpdateGoal;
using AiStudyOS.Application.Goals.Queries.GetGoals;
using AiStudyOS.Domain.Goals;
using Mediator;

namespace AiStudyOS.Api.Endpoints;

public record CreateGoalRequest(string Title, string? Description, GoalCategory Category, GoalPriority Priority, DateOnly? TargetDate);
public record UpdateGoalRequest(string Title, string? Description, GoalCategory Category, GoalPriority Priority, DateOnly? TargetDate);

public static class GoalsEndpoints
{
    public static IEndpointRouteBuilder MapGoalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/goals").WithTags("Goals");

        group.MapGet("/", async (GoalStatus? status, IMediator mediator, CancellationToken ct) =>
        {
            var goals = await mediator.Send(new GetGoalsQuery(status), ct);
            return Results.Ok(goals);
        });

        group.MapPost("/", async (CreateGoalRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var goal = await mediator.Send(new CreateGoalCommand(request.Title, request.Description, request.Category, request.Priority, request.TargetDate), ct);
            return Results.Created($"/api/v1/goals/{goal.Id}", goal);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateGoalRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var goal = await mediator.Send(new UpdateGoalCommand(id, request.Title, request.Description, request.Category, request.Priority, request.TargetDate), ct);
            return Results.Ok(goal);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteGoalCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
