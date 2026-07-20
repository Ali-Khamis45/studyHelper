using AiStudyOS.Application.Goals.Dtos;
using AiStudyOS.Domain.Goals;
using Mediator;

namespace AiStudyOS.Application.Goals.Commands.CreateGoal;

public record CreateGoalCommand(
    string Title,
    string? Description,
    GoalCategory Category,
    GoalPriority Priority,
    DateOnly? TargetDate) : ICommand<GoalDto>;
