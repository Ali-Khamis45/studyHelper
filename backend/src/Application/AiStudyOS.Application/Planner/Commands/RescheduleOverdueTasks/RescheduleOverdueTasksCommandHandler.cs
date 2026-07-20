using AiStudyOS.Application.AI.Tools;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Planner.Commands.RescheduleOverdueTasks;

/// <summary>
/// Deterministic bulk reschedule (every still-Pending overdue task moves to today) — not an AI
/// judgment call, so it doesn't go through IAiKernel; it's a direct user action exactly like
/// clicking "reschedule" on one task, just applied to every overdue one. Every mutation still goes
/// through PlannerTool, never IApplicationDbContext directly.
/// </summary>
public class RescheduleOverdueTasksCommandHandler(
    IApplicationDbContext db,
    IToolExecutor toolExecutor,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RescheduleOverdueTasksCommand, RescheduleOverdueTasksResultDto>
{
    public async ValueTask<RescheduleOverdueTasksResultDto> Handle(RescheduleOverdueTasksCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        var overdueTaskIds = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Status == DailyTaskStatus.Pending && t.Date < today)
            .Select(t => t.Id)
            .ToListAsync(ct);

        var rescheduledCount = 0;
        foreach (var taskId in overdueTaskIds)
        {
            var result = await toolExecutor.ExecuteAsync(
                "planner",
                new ToolInvocation(userId, AgentType.Recommendation, new Dictionary<string, object?>
                {
                    ["action"] = "reschedule",
                    ["taskId"] = taskId,
                    ["newDate"] = today,
                }),
                ct);

            if (result.Success) rescheduledCount++;
        }

        return new RescheduleOverdueTasksResultDto(rescheduledCount);
    }
}
