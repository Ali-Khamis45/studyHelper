using Mediator;

namespace AiStudyOS.Application.Planner.Commands.RescheduleOverdueTasks;

public record RescheduleOverdueTasksResultDto(int RescheduledCount);

public record RescheduleOverdueTasksCommand : ICommand<RescheduleOverdueTasksResultDto>;
