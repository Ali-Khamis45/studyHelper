using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Goals.Dtos;
using AiStudyOS.Domain.Planner;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Goals.Queries.GetGoals;

public class GetGoalsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetGoalsQuery, IReadOnlyList<GoalDto>>
{
    public async ValueTask<IReadOnlyList<GoalDto>> Handle(GetGoalsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var goalsQuery = db.Goals.Where(g => g.UserId == userId);
        if (query.Status is not null)
            goalsQuery = goalsQuery.Where(g => g.Status == query.Status);

        var goals = await goalsQuery.OrderBy(g => g.TargetDate ?? DateOnly.MaxValue).ThenByDescending(g => g.CreatedAtUtc).ToListAsync(ct);
        var goalIds = goals.Select(g => g.Id).ToList();

        var taskCounts = await db.DailyTasks
            .Where(t => t.GoalId != null && goalIds.Contains(t.GoalId!.Value))
            .GroupBy(t => t.GoalId!.Value)
            .Select(g => new { GoalId = g.Key, Total = g.Count(), Completed = g.Count(t => t.Status == DailyTaskStatus.Completed) })
            .ToListAsync(ct);

        var countsByGoal = taskCounts.ToDictionary(x => x.GoalId);

        return goals
            .Select(g =>
            {
                countsByGoal.TryGetValue(g.Id, out var counts);
                return GoalDto.FromDomain(g, counts?.Total ?? 0, counts?.Completed ?? 0);
            })
            .ToList();
    }
}
