using System.Text;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Planner;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.AI.Context.Providers;

/// <summary>
/// Real, DB-computed progress numbers (not model-invented) for the Analytics agent to discuss —
/// reuses PlannerQueryHelpers.GetStudyStreakAsync rather than recomputing the streak walk.
/// </summary>
public class AnalyticsSnapshotContextProvider(IApplicationDbContext db, IDateTimeProvider dateTimeProvider) : IContextProvider
{
    public string SectionName => "Progress Snapshot";

    public async Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var weekAgo = today.AddDays(-6);

        var recentTasks = await db.DailyTasks
            .Where(t => t.UserId == request.UserId && t.Date >= weekAgo && t.Date <= today)
            .ToListAsync(ct);

        var streak = await PlannerQueryHelpers.GetStudyStreakAsync(db, request.UserId, today, ct);

        var activeGoals = await db.Goals.CountAsync(g => g.UserId == request.UserId && g.Status == GoalStatus.Active, ct);
        var completedGoals = await db.Goals.CountAsync(g => g.UserId == request.UserId && g.Status == GoalStatus.Completed, ct);

        var totalThisWeek = recentTasks.Count;
        var completedThisWeek = recentTasks.Count(t => t.Status == DailyTaskStatus.Completed);
        var weeklyCompletionPercent = totalThisWeek == 0 ? 0 : Math.Round(100.0 * completedThisWeek / totalThisWeek, 1);

        var sb = new StringBuilder();
        sb.AppendLine($"- currentStudyStreak: {streak} day(s)");
        sb.AppendLine($"- tasksCompletedLast7Days: {completedThisWeek} of {totalThisWeek} ({weeklyCompletionPercent}%)");
        sb.AppendLine($"- activeGoals: {activeGoals}");
        sb.AppendLine($"- completedGoals: {completedGoals}");

        var content = sb.ToString().TrimEnd();
        return new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 90);
    }
}
