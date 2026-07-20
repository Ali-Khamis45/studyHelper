using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Goals.Dtos;
using AiStudyOS.Application.Planner;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Planner;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Goals.Commands.UpdateGoal;

public class UpdateGoalCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateGoalCommand, GoalDto>
{
    public async ValueTask<GoalDto> Handle(UpdateGoalCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var goal = await db.Goals.FirstOrDefaultAsync(g => g.Id == command.GoalId && g.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Goal), command.GoalId);

        var now = dateTimeProvider.UtcNow;
        goal.Update(command.Title.Trim(), command.Description?.Trim(), command.Category, command.Priority, command.TargetDate, now);
        await db.SaveChangesAsync(ct);

        await PlannerRecommendationInvalidator.InvalidateTodayAsync(db, userId, now, ct);

        var totalTasks = await db.DailyTasks.CountAsync(t => t.GoalId == goal.Id, ct);
        var completedTasks = await db.DailyTasks.CountAsync(t => t.GoalId == goal.Id && t.Status == DailyTaskStatus.Completed, ct);

        return GoalDto.FromDomain(goal, totalTasks, completedTasks);
    }
}
