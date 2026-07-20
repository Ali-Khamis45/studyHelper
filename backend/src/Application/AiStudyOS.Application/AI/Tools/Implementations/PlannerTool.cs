using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.AI.Tools.Implementations;

/// <summary>
/// The only path that ever mutates DailyTasks. Both AI-driven creation (from a generated
/// recommendation) and user-driven actions (complete/skip/reschedule button clicks) go through
/// this tool rather than touching IApplicationDbContext directly from a command handler — so every
/// planner write is uniformly instrumented and hard-scoped to the invoking user's own data.
/// There's no separate "User" AgentType in the domain, so all invocations — AI or human-initiated —
/// are tagged AgentType.Recommendation, the agent that owns the Planner feature area.
/// </summary>
public class PlannerTool(IApplicationDbContext db, IDateTimeProvider dateTimeProvider) : ITool
{
    public string Name => "planner";
    public string Description => "Creates and updates the study planner's daily tasks (create, complete, skip, reschedule), scoped to the invoking user.";
    public IReadOnlyList<AgentType> AllowedAgents => [AgentType.Recommendation];

    public Task<ToolResult> ExecuteAsync(ToolInvocation invocation, CancellationToken ct)
    {
        if (invocation.Parameters.GetValueOrDefault("action") is not string action)
            return Task.FromResult(ToolResult.Failed("Missing required 'action' parameter."));

        return action switch
        {
            "create" => CreateAsync(invocation, ct),
            "complete" => CompleteAsync(invocation, ct),
            "skip" => SkipAsync(invocation, ct),
            "reschedule" => RescheduleAsync(invocation, ct),
            "clearPendingAiGenerated" => ClearPendingAiGeneratedAsync(invocation, ct),
            _ => Task.FromResult(ToolResult.Failed($"Unknown planner action '{action}'.")),
        };
    }

    /// <summary>
    /// Removes untouched (still-Pending) AI-generated tasks for a given date before regenerating
    /// today's recommendation, so re-generating doesn't pile up duplicate task sets. Tasks the user
    /// already completed or skipped are real history and are left alone.
    /// </summary>
    private async Task<ToolResult> ClearPendingAiGeneratedAsync(ToolInvocation invocation, CancellationToken ct)
    {
        if (invocation.Parameters.GetValueOrDefault("date") is not DateOnly date)
            return ToolResult.Failed("Missing or invalid 'date' parameter.");

        var stale = await db.DailyTasks
            .Where(t => t.UserId == invocation.UserId && t.Date == date && t.Source == TaskSource.AiGenerated && t.Status == DailyTaskStatus.Pending)
            .ToListAsync(ct);

        db.DailyTasks.RemoveRange(stale);
        await db.SaveChangesAsync(ct);

        return ToolResult.Ok(stale.Count);
    }

    private async Task<ToolResult> CreateAsync(ToolInvocation invocation, CancellationToken ct)
    {
        var parameters = invocation.Parameters;

        if (parameters.GetValueOrDefault("title") is not string title || string.IsNullOrWhiteSpace(title))
            return ToolResult.Failed("Missing required 'title' parameter.");
        if (parameters.GetValueOrDefault("date") is not DateOnly date)
            return ToolResult.Failed("Missing or invalid 'date' parameter.");

        var goalId = parameters.GetValueOrDefault("goalId") as Guid?;
        var reasoning = parameters.GetValueOrDefault("reasoning") as string;
        var estimatedMinutes = parameters.GetValueOrDefault("estimatedMinutes") is int minutes ? minutes : 30;
        var source = parameters.GetValueOrDefault("source") is TaskSource s ? s : TaskSource.Manual;

        // Defensive: an AI-supplied goalId that doesn't belong to this user gets dropped rather
        // than failing the whole task — a slightly-wrong link shouldn't block an otherwise valid task.
        if (goalId is not null && !await db.Goals.AnyAsync(g => g.Id == goalId && g.UserId == invocation.UserId, ct))
            goalId = null;

        var task = DailyTask.Create(invocation.UserId, goalId, title, reasoning, date, estimatedMinutes, source, dateTimeProvider.UtcNow);
        db.DailyTasks.Add(task);
        await db.SaveChangesAsync(ct);

        return ToolResult.Ok(task.Id);
    }

    private async Task<ToolResult> CompleteAsync(ToolInvocation invocation, CancellationToken ct)
    {
        var task = await FindTaskAsync(invocation, ct);
        if (task is null) return ToolResult.Failed("Task not found.");

        var now = dateTimeProvider.UtcNow;
        task.Complete(now);
        await db.SaveChangesAsync(ct);
        await PlannerRecommendationInvalidator.InvalidateTodayAsync(db, invocation.UserId, now, ct);
        return ToolResult.Ok(task.Id);
    }

    private async Task<ToolResult> SkipAsync(ToolInvocation invocation, CancellationToken ct)
    {
        var task = await FindTaskAsync(invocation, ct);
        if (task is null) return ToolResult.Failed("Task not found.");

        var now = dateTimeProvider.UtcNow;
        task.Skip(now);
        await db.SaveChangesAsync(ct);
        await PlannerRecommendationInvalidator.InvalidateTodayAsync(db, invocation.UserId, now, ct);
        return ToolResult.Ok(task.Id);
    }

    private async Task<ToolResult> RescheduleAsync(ToolInvocation invocation, CancellationToken ct)
    {
        var task = await FindTaskAsync(invocation, ct);
        if (task is null) return ToolResult.Failed("Task not found.");

        if (invocation.Parameters.GetValueOrDefault("newDate") is not DateOnly newDate)
            return ToolResult.Failed("Missing or invalid 'newDate' parameter.");

        var now = dateTimeProvider.UtcNow;
        task.Reschedule(newDate, now);
        await db.SaveChangesAsync(ct);
        await PlannerRecommendationInvalidator.InvalidateTodayAsync(db, invocation.UserId, now, ct);
        return ToolResult.Ok(task.Id);
    }

    private async Task<DailyTask?> FindTaskAsync(ToolInvocation invocation, CancellationToken ct)
    {
        if (invocation.Parameters.GetValueOrDefault("taskId") is not Guid taskId)
            return null;

        // Scoped to invocation.UserId in the query itself: this is the actual enforcement point
        // that makes cross-user mutation impossible, regardless of what a caller passes in.
        return await db.DailyTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == invocation.UserId, ct);
    }
}
