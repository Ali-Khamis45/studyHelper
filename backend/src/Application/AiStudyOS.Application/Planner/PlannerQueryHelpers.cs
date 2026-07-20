using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner.Dtos;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Planner;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Planner;

/// <summary>
/// Shared between GenerateDailyRecommendationCommandHandler (just created a recommendation) and
/// GetTodayQueryHandler (reading one that already exists) so both return an identically-shaped
/// TodayPlanDto without duplicating the task/deadline fetch logic.
/// </summary>
public static class PlannerQueryHelpers
{
    public static async Task<TodayPlanDto> BuildTodayPlanAsync(
        IApplicationDbContext db, Guid userId, DateOnly today, PlannerRecommendation? recommendation, CancellationToken ct)
    {
        var tasks = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Date == today)
            .OrderBy(t => t.Status == DailyTaskStatus.Pending ? 0 : 1)
            .ThenByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);

        var goalTitles = await GetGoalTitlesAsync(db, userId, tasks.Select(t => t.GoalId), ct);
        var taskDtos = tasks.Select(t => DailyTaskDto.FromDomain(t, t.GoalId is { } id ? goalTitles.GetValueOrDefault(id) : null)).ToList();

        var deadlines = await GetUpcomingDeadlinesAsync(db, userId, today, ct);

        return new TodayPlanDto(
            recommendation is null ? null : PlannerRecommendationDto.FromDomain(recommendation),
            taskDtos,
            deadlines);
    }

    public static async Task<IReadOnlyList<UpcomingDeadlineDto>> GetUpcomingDeadlinesAsync(IApplicationDbContext db, Guid userId, DateOnly today, CancellationToken ct)
    {
        var goals = await db.Goals
            .Where(g => g.UserId == userId && g.Status == GoalStatus.Active && g.TargetDate != null && g.TargetDate >= today)
            .OrderBy(g => g.TargetDate)
            .Take(5)
            .ToListAsync(ct);

        return goals
            .Select(g => new UpcomingDeadlineDto(g.Id, g.Title, g.TargetDate!.Value, g.TargetDate.Value.DayNumber - today.DayNumber))
            .ToList();
    }

    public static async Task<Dictionary<Guid, string>> GetGoalTitlesAsync(IApplicationDbContext db, Guid userId, IEnumerable<Guid?> goalIds, CancellationToken ct)
    {
        var ids = goalIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (ids.Count == 0) return [];

        return await db.Goals
            .Where(g => g.UserId == userId && ids.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, g => g.Title, ct);
    }
}
