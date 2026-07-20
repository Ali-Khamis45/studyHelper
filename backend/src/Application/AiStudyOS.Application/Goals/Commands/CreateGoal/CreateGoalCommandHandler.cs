using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Goals.Dtos;
using AiStudyOS.Application.Planner;
using AiStudyOS.Domain.Goals;
using Mediator;

namespace AiStudyOS.Application.Goals.Commands.CreateGoal;

public class CreateGoalCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateGoalCommand, GoalDto>
{
    public async ValueTask<GoalDto> Handle(CreateGoalCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var now = dateTimeProvider.UtcNow;

        var goal = Goal.Create(userId, command.Title.Trim(), command.Description?.Trim(), command.Category, command.Priority, command.TargetDate, now);

        db.Goals.Add(goal);
        await db.SaveChangesAsync(ct);

        await PlannerRecommendationInvalidator.InvalidateTodayAsync(db, userId, now, ct);

        return GoalDto.FromDomain(goal, totalTasks: 0, completedTasks: 0);
    }
}
