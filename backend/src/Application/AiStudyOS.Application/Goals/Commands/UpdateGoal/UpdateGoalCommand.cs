using AiStudyOS.Application.Goals.Dtos;
using AiStudyOS.Domain.Goals;
using Mediator;

namespace AiStudyOS.Application.Goals.Commands.UpdateGoal;

public record UpdateGoalCommand(
    Guid GoalId,
    string Title,
    string? Description,
    GoalCategory Category,
    GoalPriority Priority,
    DateOnly? TargetDate) : ICommand<GoalDto>;
