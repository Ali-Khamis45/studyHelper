using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Analytics.Dtos.Charts;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Quiz.Dtos;
using AiStudyOS.Application.Planner;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;
using AiStudyOS.Domain.Quiz;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Analytics;

/// <summary>
/// All Analytics computation lives here, in the Application layer, over real rows in the existing
/// tables (Goals, DailyTasks, PlannerRecommendations, Conversations/Messages, Quizzes/Attempts,
/// TopicMastery(History), AiTelemetryEvents) — nothing here is randomly generated or hand-waved.
/// Every method aggregates server-side (SQL SUM/COUNT/AVG/GROUP BY) wherever EF Core can translate
/// it; the few spots that materialize rows first (streak-gap detection, session-length averaging)
/// do so only over a single user's naturally small row count, not the whole table.
/// </summary>
public static class AnalyticsQueryHelpers
{
    public static async Task<StudyTimeStatsDto> ComputeStudyTimeAsync(IApplicationDbContext db, Guid userId, DateOnly today, CancellationToken ct)
    {
        var weekStart = today.AddDays(-6);
        var monthStart = today.AddDays(-29);

        var completed = db.DailyTasks.Where(t => t.UserId == userId && t.Status == DailyTaskStatus.Completed);

        var daily = await completed.Where(t => t.Date == today).SumAsync(t => t.EstimatedMinutes, ct);
        var weekly = await completed.Where(t => t.Date >= weekStart && t.Date <= today).SumAsync(t => t.EstimatedMinutes, ct);
        var monthly = await completed.Where(t => t.Date >= monthStart && t.Date <= today).SumAsync(t => t.EstimatedMinutes, ct);

        return new StudyTimeStatsDto(daily, weekly, monthly);
    }

    public static async Task<TaskStatsDto> ComputeTaskStatsAsync(IApplicationDbContext db, Guid userId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var tasksQuery = db.DailyTasks.Where(t => t.UserId == userId && t.Date >= from && t.Date <= to);

        var stats = await tasksQuery
            .GroupBy(t => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Completed = g.Count(t => t.Status == DailyTaskStatus.Completed),
                Skipped = g.Count(t => t.Status == DailyTaskStatus.Skipped),
                Pending = g.Count(t => t.Status == DailyTaskStatus.Pending),
                Rescheduled = g.Count(t => t.RescheduleCount > 0),
            })
            .FirstOrDefaultAsync(ct);

        if (stats is null)
            return new TaskStatsDto(0, 0, 0, 0, 0, 0);

        var completionRate = stats.Total == 0 ? 0 : Math.Round(100.0 * stats.Completed / stats.Total, 1);
        return new TaskStatsDto(stats.Completed, stats.Skipped, stats.Rescheduled, stats.Pending, stats.Total, completionRate);
    }

    public static async Task<IReadOnlyList<ChartPointDto>> ComputeDailyActivityAsync(IApplicationDbContext db, Guid userId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var completedByDay = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Status == DailyTaskStatus.Completed && t.Date >= from && t.Date <= to)
            .GroupBy(t => t.Date)
            .Select(g => new { Date = g.Key, Minutes = g.Sum(t => t.EstimatedMinutes) })
            .ToListAsync(ct);

        var byDate = completedByDay.ToDictionary(x => x.Date, x => x.Minutes);

        var points = new List<ChartPointDto>();
        for (var date = from; date <= to; date = date.AddDays(1))
            points.Add(new ChartPointDto(date.ToString("yyyy-MM-dd"), byDate.GetValueOrDefault(date, 0)));

        return points;
    }

    public static async Task<GoalAnalyticsDto> ComputeGoalAnalyticsAsync(IApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var goals = await db.Goals.Where(g => g.UserId == userId).OrderByDescending(g => g.CreatedAtUtc).ToListAsync(ct);
        var goalIds = goals.Select(g => g.Id).ToList();

        var taskCounts = await db.DailyTasks
            .Where(t => t.GoalId != null && goalIds.Contains(t.GoalId!.Value))
            .GroupBy(t => t.GoalId!.Value)
            .Select(g => new { GoalId = g.Key, Total = g.Count(), Completed = g.Count(t => t.Status == DailyTaskStatus.Completed) })
            .ToListAsync(ct);

        var countsByGoal = taskCounts.ToDictionary(x => x.GoalId);

        var items = goals.Select(g =>
        {
            countsByGoal.TryGetValue(g.Id, out var counts);
            var progress = counts is null || counts.Total == 0 ? 0 : (int)Math.Round(100.0 * counts.Completed / counts.Total);
            return new GoalProgressItemDto(g.Id, g.Title, g.Status.ToString(), progress);
        }).ToList();

        var completedGoals = goals.Count(g => g.Status == GoalStatus.Completed);
        var completionPercent = goals.Count == 0 ? 0 : Math.Round(100.0 * completedGoals / goals.Count, 1);

        return new GoalAnalyticsDto(goals.Count, completedGoals, completionPercent, items);
    }

    public static async Task<StreakAnalyticsDto> ComputeStreakAnalyticsAsync(IApplicationDbContext db, Guid userId, DateOnly today, AnalyticsOptions options, CancellationToken ct)
    {
        var currentStreak = await PlannerQueryHelpers.GetStudyStreakAsync(db, userId, today, ct);

        // Longest streak needs gap detection across the full completion-date history, which EF
        // Core's LINQ provider can't express as a single SQL aggregate — the distinct-date list
        // itself is naturally small (bounded by days-with-any-completion, not total tasks), so
        // materializing just that and walking it in C# is the honest tradeoff here.
        var completionDates = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Status == DailyTaskStatus.Completed)
            .Select(t => t.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(ct);

        var longestStreak = ComputeLongestRun(completionDates);

        var since = today.AddDays(-(options.HeatmapWindowDays - 1));
        var heatmapCounts = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Status == DailyTaskStatus.Completed && t.Date >= since && t.Date <= today)
            .GroupBy(t => t.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var heatmap = heatmapCounts.Select(x => new HeatmapCellDto(x.Date.ToString("yyyy-MM-dd"), x.Count)).ToList();

        return new StreakAnalyticsDto(currentStreak, longestStreak, heatmap);
    }

    private static int ComputeLongestRun(IReadOnlyList<DateOnly> sortedDistinctDates)
    {
        if (sortedDistinctDates.Count == 0) return 0;

        var longest = 1;
        var current = 1;
        for (var i = 1; i < sortedDistinctDates.Count; i++)
        {
            if (sortedDistinctDates[i].DayNumber == sortedDistinctDates[i - 1].DayNumber + 1)
            {
                current++;
                longest = Math.Max(longest, current);
            }
            else
            {
                current = 1;
            }
        }

        return longest;
    }

    public static async Task<QuizAnalyticsDto> ComputeQuizAnalyticsAsync(IApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var scored = db.QuizAttempts.Where(a => a.UserId == userId && a.Status == AttemptStatus.Completed && a.Score != null);

        var attemptCount = await scored.CountAsync(ct);
        if (attemptCount == 0)
            return new QuizAnalyticsDto(0, 0, 0, 0, [], []);

        var averageScore = Math.Round(await scored.AverageAsync(a => a.Score!.Value, ct), 1);
        var highestScore = await scored.MaxAsync(a => a.Score!.Value, ct);
        var lowestScore = await scored.MinAsync(a => a.Score!.Value, ct);

        var rows = await scored.OrderBy(a => a.CompletedAtUtc).Select(a => new { a.CompletedAtUtc, Score = a.Score!.Value }).ToListAsync(ct);
        var trend = rows.Select(r => new ChartPointDto(r.CompletedAtUtc!.Value.ToString("yyyy-MM-dd"), r.Score)).ToList();

        var buckets = new (string Label, double Low, double High)[] { ("0-20", 0, 20), ("20-40", 20, 40), ("40-60", 40, 60), ("60-80", 60, 80), ("80-100", 80, 100) };
        var distribution = buckets
            .Select(b => new DistributionBucketDto(b.Label, rows.Count(r => r.Score >= b.Low && (b.High == 100 ? r.Score <= b.High : r.Score < b.High))))
            .ToList();

        return new QuizAnalyticsDto(attemptCount, averageScore, highestScore, lowestScore, trend, distribution);
    }

    public static async Task<MasteryAnalyticsDto> ComputeMasteryAnalyticsAsync(IApplicationDbContext db, Guid userId, AnalyticsOptions options, CancellationToken ct)
    {
        var allMastery = await db.TopicMasteries.Where(m => m.UserId == userId).OrderByDescending(m => m.MasteryScore).ToListAsync(ct);

        var weak = allMastery.Where(m => m.MasteryScore < 0.6).OrderBy(m => m.MasteryScore).Take(options.WeakStrongTopicsTake).Select(TopicMasteryDto.FromDomain).ToList();
        var strong = allMastery.Where(m => m.MasteryScore >= options.StrongTopicMasteryThreshold).OrderByDescending(m => m.MasteryScore).Take(options.WeakStrongTopicsTake).Select(TopicMasteryDto.FromDomain).ToList();
        var radar = allMastery.Take(options.RadarMaxAxes).Select(m => new RadarAxisDto(m.Topic, Math.Round(m.MasteryScore * 100, 1))).ToList();

        var evolutionRows = await db.TopicMasteryHistories
            .Where(h => h.UserId == userId)
            .GroupBy(h => h.RecordedAtUtc.Date)
            .Select(g => new { Date = g.Key, Average = g.Average(h => h.MasteryScore) })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        var evolution = evolutionRows.Select(r => new ChartPointDto(r.Date.ToString("yyyy-MM-dd"), Math.Round(r.Average * 100, 1))).ToList();

        return new MasteryAnalyticsDto(weak, strong, radar, evolution);
    }

    public static async Task<MentorAnalyticsDto> ComputeMentorAnalyticsAsync(IApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var conversations = await db.Conversations
            .Where(c => c.UserId == userId)
            .Select(c => new { c.CreatedAtUtc, c.LastMessageAtUtc, c.MessageCount })
            .ToListAsync(ct);

        var messageCount = conversations.Sum(c => c.MessageCount);
        var withActivity = conversations.Where(c => c.LastMessageAtUtc.HasValue).ToList();
        var avgSessionMinutes = withActivity.Count == 0
            ? 0
            : Math.Round(withActivity.Average(c => (c.LastMessageAtUtc!.Value - c.CreatedAtUtc).TotalMinutes), 1);

        return new MentorAnalyticsDto(conversations.Count, messageCount, avgSessionMinutes);
    }

    public static async Task<AiAnalyticsDto> ComputeAiAnalyticsAsync(IApplicationDbContext db, Guid userId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var events = db.AiTelemetryEvents.Where(e => e.UserId == userId && e.CreatedAtUtc >= fromUtc && e.CreatedAtUtc <= toUtc);

        var totals = await events
            .GroupBy(e => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Successes = g.Count(e => e.Success),
                AvgLatency = g.Average(e => (double)e.LatencyMs),
                PromptTokens = g.Sum(e => e.PromptTokens),
                CompletionTokens = g.Sum(e => e.CompletionTokens),
            })
            .FirstOrDefaultAsync(ct);

        if (totals is null || totals.Total == 0)
            return new AiAnalyticsDto(0, 0, 0, 0, 0, 0, []);

        var byProvider = await events
            .GroupBy(e => e.ProviderKey)
            .Select(g => new ProviderStatDto(g.Key, g.Count(), Math.Round(g.Average(e => (double)e.LatencyMs), 0)))
            .ToListAsync(ct);

        var successRate = Math.Round(100.0 * totals.Successes / totals.Total, 1);

        return new AiAnalyticsDto(totals.Total, successRate, Math.Round(100.0 - successRate, 1), Math.Round(totals.AvgLatency, 0), totals.PromptTokens, totals.CompletionTokens, byProvider);
    }

    public static async Task<PlannerAnalyticsDto> ComputePlannerAnalyticsAsync(IApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var recommendations = await db.PlannerRecommendations
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.GeneratedAt)
            .Select(r => new { r.GeneratedAt, r.ConfidenceScore, r.GenerationTimeMs, r.RecommendedTaskId })
            .ToListAsync(ct);

        if (recommendations.Count == 0)
            return new PlannerAnalyticsDto(0, 0, 0, 0, []);

        var withTask = recommendations.Where(r => r.RecommendedTaskId.HasValue).ToList();
        int accepted = 0;
        if (withTask.Count > 0)
        {
            var taskIds = withTask.Select(r => r.RecommendedTaskId!.Value).ToList();
            var completedTaskIds = await db.DailyTasks
                .Where(t => taskIds.Contains(t.Id) && t.Status == DailyTaskStatus.Completed)
                .Select(t => t.Id)
                .ToListAsync(ct);
            accepted = completedTaskIds.Count;
        }

        var acceptanceRate = withTask.Count == 0 ? 0 : Math.Round(100.0 * accepted / withTask.Count, 1);
        var avgConfidence = Math.Round(recommendations.Average(r => r.ConfidenceScore), 2);
        var avgGenerationTime = Math.Round(recommendations.Average(r => r.GenerationTimeMs), 0);
        var trend = recommendations.Select(r => new ChartPointDto(r.GeneratedAt.ToString("yyyy-MM-dd"), r.ConfidenceScore)).ToList();

        return new PlannerAnalyticsDto(recommendations.Count, acceptanceRate, avgConfidence, avgGenerationTime, trend);
    }

    public static async Task<IReadOnlyList<PieSliceDto>> ComputeTaskStatusDistributionAsync(IApplicationDbContext db, Guid userId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var stats = await ComputeTaskStatsAsync(db, userId, from, to, ct);
        return
        [
            new PieSliceDto("Completed", stats.Completed),
            new PieSliceDto("Skipped", stats.Skipped),
            new PieSliceDto("Pending", stats.Pending),
        ];
    }

    /// <summary>A unified activity feed across every module — goals created, tasks completed, quizzes taken, conversations started — each event drawn straight from its own table's timestamp, nothing synthesized.</summary>
    public static async Task<IReadOnlyList<TimelineEventDto>> ComputeTimelineAsync(IApplicationDbContext db, Guid userId, int take, CancellationToken ct)
    {
        var goalEvents = await db.Goals.Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAtUtc).Take(take)
            .Select(g => new TimelineEventDto(g.CreatedAtUtc, "GoalCreated", g.Title))
            .ToListAsync(ct);

        var taskEvents = await db.DailyTasks.Where(t => t.UserId == userId && t.Status == DailyTaskStatus.Completed && t.CompletedAtUtc != null)
            .OrderByDescending(t => t.CompletedAtUtc).Take(take)
            .Select(t => new TimelineEventDto(t.CompletedAtUtc!.Value, "TaskCompleted", t.Title))
            .ToListAsync(ct);

        var quizEvents = await db.QuizAttempts.Where(a => a.UserId == userId && a.Status == AttemptStatus.Completed && a.CompletedAtUtc != null)
            .OrderByDescending(a => a.CompletedAtUtc).Take(take)
            .Join(db.Quizzes, a => a.QuizId, q => q.Id, (a, q) => new TimelineEventDto(a.CompletedAtUtc!.Value, "QuizCompleted", $"{q.Title} — {a.Score}%"))
            .ToListAsync(ct);

        var conversationEvents = await db.Conversations.Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAtUtc).Take(take)
            .Select(c => new TimelineEventDto(c.CreatedAtUtc, "ConversationStarted", c.Title))
            .ToListAsync(ct);

        return goalEvents.Concat(taskEvents).Concat(quizEvents).Concat(conversationEvents)
            .OrderByDescending(e => e.OccurredAtUtc)
            .Take(take)
            .ToList();
    }
}
