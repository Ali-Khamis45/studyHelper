using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner.Dtos;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Planner;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Planner;

/// <summary>
/// Shared between GenerateDailyRecommendationCommandHandler (just created a recommendation) and
/// GetTodayQueryHandler (reading one that already exists) so both return an identically-shaped
/// TodayPlanDto without duplicating the task/deadline/stats fetch logic.
/// </summary>
public static class PlannerQueryHelpers
{
    private const int DefaultStreakLookbackDays = 365;

    public static async Task<TodayPlanDto> BuildTodayPlanAsync(
        IApplicationDbContext db, Guid userId, DateOnly today, PlannerRecommendation? recommendation, CancellationToken ct, int streakLookbackDays = DefaultStreakLookbackDays)
    {
        var tasks = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Date == today)
            .OrderBy(t => t.Status == DailyTaskStatus.Pending ? 0 : 1)
            .ThenByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);

        var goalTitles = await GetGoalTitlesAsync(db, userId, tasks.Select(t => t.GoalId), ct);
        var taskDtos = tasks.Select(t => DailyTaskDto.FromDomain(t, t.GoalId is { } id ? goalTitles.GetValueOrDefault(id) : null, today)).ToList();

        var deadlines = await GetUpcomingDeadlinesAsync(db, userId, today, ct);
        var overdueTasks = await GetOverdueTasksAsync(db, userId, today, ct);
        var studyStreak = await GetStudyStreakAsync(db, userId, today, ct, streakLookbackDays);

        var (dailyCompletionPercent, dailyFocusScore) = ComputeDailyStats(tasks);

        return new TodayPlanDto(
            recommendation is null ? null : PlannerRecommendationDto.FromDomain(recommendation),
            taskDtos,
            deadlines,
            overdueTasks,
            dailyCompletionPercent,
            dailyFocusScore,
            studyStreak);
    }

    /// <summary>
    /// DailyCompletionPercent: Completed / all of today's tasks. DailyFocusScore: minutes-weighted
    /// version of the same idea (completed minutes / planned minutes) — a task worth 90 minutes
    /// contributes more to "focus" than one worth 15, unlike a flat task-count percentage.
    /// </summary>
    private static (double CompletionPercent, int FocusScore) ComputeDailyStats(IReadOnlyList<DailyTask> tasks)
    {
        if (tasks.Count == 0) return (0, 0);

        var completionPercent = Math.Round(100.0 * tasks.Count(t => t.Status == DailyTaskStatus.Completed) / tasks.Count, 1);

        var plannedMinutes = tasks.Sum(t => t.EstimatedMinutes);
        var completedMinutes = tasks.Where(t => t.Status == DailyTaskStatus.Completed).Sum(t => t.EstimatedMinutes);
        var focusScore = plannedMinutes == 0 ? 0 : (int)Math.Round(100.0 * completedMinutes / plannedMinutes);

        return (completionPercent, focusScore);
    }

    private static async Task<IReadOnlyList<DailyTaskDto>> GetOverdueTasksAsync(IApplicationDbContext db, Guid userId, DateOnly today, CancellationToken ct)
    {
        var overdue = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Status == DailyTaskStatus.Pending && t.Date < today)
            .OrderBy(t => t.Date)
            .ToListAsync(ct);

        if (overdue.Count == 0) return [];

        var goalTitles = await GetGoalTitlesAsync(db, userId, overdue.Select(t => t.GoalId), ct);
        return overdue.Select(t => DailyTaskDto.FromDomain(t, t.GoalId is { } id ? goalTitles.GetValueOrDefault(id) : null, today)).ToList();
    }

    /// <summary>
    /// Consecutive days (walking back from today) with at least one Completed task. Today itself
    /// only counts once it actually has a completion — but its absence doesn't break a streak still
    /// in progress, since today isn't over yet; the walk simply starts from yesterday instead.
    /// </summary>
    public static async Task<int> GetStudyStreakAsync(IApplicationDbContext db, Guid userId, DateOnly today, CancellationToken ct, int lookbackDays = DefaultStreakLookbackDays)
    {
        var since = today.AddDays(-lookbackDays);

        var completionDates = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Status == DailyTaskStatus.Completed && t.Date >= since && t.Date <= today)
            .Select(t => t.Date)
            .Distinct()
            .ToListAsync(ct);

        var completionDateSet = completionDates.ToHashSet();

        var cursor = completionDateSet.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;
        while (completionDateSet.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
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
