using AiStudyOS.Application.AI.Tools;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;
using Mediator;

namespace AiStudyOS.Application.Planner.Commands.CompleteTask;

public class CompleteTaskCommandHandler(IToolExecutor toolExecutor, ICurrentUserService currentUser) : ICommandHandler<CompleteTaskCommand, bool>
{
    public async ValueTask<bool> Handle(CompleteTaskCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var result = await toolExecutor.ExecuteAsync(
            "planner",
            new ToolInvocation(userId, AgentType.Recommendation, new Dictionary<string, object?> { ["action"] = "complete", ["taskId"] = command.TaskId }),
            ct);

        if (!result.Success)
            throw new NotFoundException(nameof(DailyTask), command.TaskId);

        return true;
    }
}
