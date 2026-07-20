using AiStudyOS.Application.AI.Tools;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;
using Mediator;

namespace AiStudyOS.Application.Planner.Commands.RescheduleTask;

public class RescheduleTaskCommandHandler(IToolExecutor toolExecutor, ICurrentUserService currentUser) : ICommandHandler<RescheduleTaskCommand, bool>
{
    public async ValueTask<bool> Handle(RescheduleTaskCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var result = await toolExecutor.ExecuteAsync(
            "planner",
            new ToolInvocation(userId, AgentType.Recommendation, new Dictionary<string, object?> { ["action"] = "reschedule", ["taskId"] = command.TaskId, ["newDate"] = command.NewDate }),
            ct);

        if (!result.Success)
            throw new NotFoundException(nameof(DailyTask), command.TaskId);

        return true;
    }
}
