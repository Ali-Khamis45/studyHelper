using Mediator;

namespace AiStudyOS.Application.Planner.Commands.SkipTask;

public record SkipTaskCommand(Guid TaskId) : ICommand<bool>;
