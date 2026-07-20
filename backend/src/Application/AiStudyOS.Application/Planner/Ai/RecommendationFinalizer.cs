using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.AI.Tools;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner.Dtos;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;

namespace AiStudyOS.Application.Planner.Ai;

/// <summary>
/// Turns a parsed RecommendationResult into persisted DailyTasks + a PlannerRecommendation and
/// returns the resulting TodayPlanDto. The one and only place either the non-streaming
/// (GenerateDailyRecommendationCommandHandler) or streaming (RecommendationStreamer) execution path
/// is allowed to create tasks/recommendations, so both converge on identical persistence behavior.
/// </summary>
public static class RecommendationFinalizer
{
    public static async Task<TodayPlanDto> FinalizeAsync(
        IApplicationDbContext db,
        IToolExecutor toolExecutor,
        Guid userId,
        DateOnly today,
        RecommendationResult data,
        AiTelemetryRecord telemetry,
        string rawContent,
        DateTime nowUtc,
        CancellationToken ct)
    {
        // Clear untouched tasks from a previous generation today before creating the new batch —
        // both this and "create" below go through PlannerTool, never IApplicationDbContext directly.
        await toolExecutor.ExecuteAsync(
            "planner",
            new ToolInvocation(userId, AgentType.Recommendation, new Dictionary<string, object?> { ["action"] = "clearPendingAiGenerated", ["date"] = today }),
            ct);

        var createdTaskIds = new List<Guid>();
        foreach (var proposedTask in data.Tasks)
        {
            var result = await toolExecutor.ExecuteAsync(
                "planner",
                new ToolInvocation(userId, AgentType.Recommendation, new Dictionary<string, object?>
                {
                    ["action"] = "create",
                    ["title"] = proposedTask.Title,
                    ["date"] = today,
                    ["goalId"] = proposedTask.GoalId,
                    ["reasoning"] = proposedTask.Reasoning,
                    ["estimatedMinutes"] = proposedTask.EstimatedMinutes,
                    ["energyLevel"] = proposedTask.EnergyLevel,
                    ["source"] = TaskSource.AiGenerated,
                }),
                ct);

            if (result is { Success: true, Data: Guid taskId })
                createdTaskIds.Add(taskId);
        }

        var recommendation = PlannerRecommendation.Create(
            userId,
            today,
            data.SituationAnalysis,
            data.GoalAlignment,
            data.Evidence,
            data.Recommendation,
            data.ImmediateNextAction,
            createdTaskIds.FirstOrDefault() is { } firstId && firstId != Guid.Empty ? firstId : null,
            telemetry.Model,
            telemetry.ProviderKey,
            telemetry.PromptVersion,
            data.ConfidenceScore,
            data.RecommendationReason,
            telemetry.LatencyMs,
            rawContent,
            nowUtc);

        db.PlannerRecommendations.Add(recommendation);
        await db.SaveChangesAsync(ct);

        return await PlannerQueryHelpers.BuildTodayPlanAsync(db, userId, today, recommendation, ct);
    }
}
