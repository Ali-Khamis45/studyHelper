using Mediator;

namespace AiStudyOS.Application.Planner.Commands.RescheduleTask;

public record RescheduleTaskCommand(Guid TaskId, DateOnly NewDate) : ICommand<bool>;
