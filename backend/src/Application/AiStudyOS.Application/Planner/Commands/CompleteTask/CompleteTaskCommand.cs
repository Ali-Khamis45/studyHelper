using Mediator;

namespace AiStudyOS.Application.Planner.Commands.CompleteTask;

public record CompleteTaskCommand(Guid TaskId) : ICommand<bool>;
