using System.Runtime.CompilerServices;
using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Tools;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner.Ai;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.Planner.Streaming;

public class RecommendationStreamer(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel,
    IToolExecutor toolExecutor) : IRecommendationStreamer
{
    public async IAsyncEnumerable<RecommendationStreamEvent> StreamTodayRecommendationAsync([EnumeratorCancellation] CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        var (_, context, prompt) = await RecommendationPreparation.PrepareAsync(agentRegistry, contextBuilder, promptLibrary, userId, ct);
        var request = new KernelRequest(AgentType.Recommendation, prompt, context, prompt.ExpectedJsonSchema, UserId: userId);

        // AiKernel parses/validates/retries internally — identically to ExecuteAsync — so this
        // class only forwards deltas as they arrive and reads the already-parsed result off the
        // final chunk. No JSON parsing happens here.
        KernelResult<RecommendationResult>? result = null;

        await foreach (var chunk in aiKernel.ExecuteStreamAsync<RecommendationResult>(request, ct))
        {
            if (!chunk.IsFinal)
            {
                yield return new RecommendationDeltaEvent(chunk.DeltaContent);
                continue;
            }

            result = chunk.Result;
        }

        if (result is null || !result.Success || result.Data is null)
        {
            var reason = result?.Errors is { Count: > 0 } errors ? string.Join("; ", errors) : "AI generation failed while streaming.";
            yield return new RecommendationErrorEvent(reason);
            yield break;
        }

        var plan = await RecommendationFinalizer.FinalizeAsync(
            db, toolExecutor, userId, today, result.Data, result.Telemetry, result.RawContent, dateTimeProvider.UtcNow, ct);

        yield return new RecommendationCompleteEvent(plan);
    }
}
