using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Goals.Commands.DeleteGoal;

public class DeleteGoalCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider) : ICommandHandler<DeleteGoalCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteGoalCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var goal = await db.Goals.FirstOrDefaultAsync(g => g.Id == command.GoalId && g.UserId == userId, ct);
        if (goal is null) return false;

        db.Goals.Remove(goal);
        await db.SaveChangesAsync(ct);

        await PlannerRecommendationInvalidator.InvalidateTodayAsync(db, userId, dateTimeProvider.UtcNow, ct);

        return true;
    }
}
