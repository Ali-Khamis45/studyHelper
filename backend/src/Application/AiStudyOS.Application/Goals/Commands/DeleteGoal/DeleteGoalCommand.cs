using Mediator;

namespace AiStudyOS.Application.Goals.Commands.DeleteGoal;

public record DeleteGoalCommand(Guid GoalId) : ICommand<bool>;
