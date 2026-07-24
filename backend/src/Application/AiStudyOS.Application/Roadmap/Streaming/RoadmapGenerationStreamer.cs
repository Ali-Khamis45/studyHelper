using System.Runtime.CompilerServices;
using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Roadmap.Ai;
using AiStudyOS.Application.Roadmap.Dtos;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.Roadmap.Streaming;

public class RoadmapGenerationStreamer(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel) : IRoadmapGenerationStreamer
{
    public async IAsyncEnumerable<RoadmapGenerationStreamEvent> StreamGenerateAsync(RoadmapProfile profile, [EnumeratorCancellation] CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var (_, context, prompt) = await RoadmapPreparation.PrepareAsync(agentRegistry, contextBuilder, promptLibrary, userId, profile, ct);
        var request = new KernelRequest(AgentType.RoadmapGenerator, prompt, context, prompt.ExpectedJsonSchema, UserId: userId);

        KernelResult<RoadmapGenerationResult>? result = null;

        await foreach (var chunk in aiKernel.ExecuteStreamAsync<RoadmapGenerationResult>(request, ct))
        {
            if (!chunk.IsFinal)
            {
                yield return new RoadmapGenerationDeltaEvent(chunk.DeltaContent);
                continue;
            }

            result = chunk.Result;
        }

        if (result is null || !result.Success || result.Data is null)
        {
            var reason = result?.Errors is { Count: > 0 } errors ? string.Join("; ", errors) : "AI generation failed while streaming.";
            yield return new RoadmapGenerationErrorEvent(reason);
            yield break;
        }

        // Finalization can still reject the model's output for a reason AiKernel's own JSON-parse
        // retry never sees (a *validly-shaped* JSON object missing a business-required field, e.g. a
        // topic with no title) — a real, observed failure mode on a payload this large. By the time
        // we're here the HTTP response has already started streaming, so an uncaught exception can't
        // be turned into a clean error response by GlobalExceptionHandler the way the non-streaming
        // path gets it for free; it would instead surface as a broken connection the client just
        // hangs on. Catching it here and yielding a normal error event keeps this path's failure
        // mode identical to every other failure this streamer already reports gracefully.
        RoadmapDto? roadmap = null;
        string? finalizeError = null;
        try
        {
            roadmap = await RoadmapFinalizer.FinalizeAsync(db, userId, profile, result.Data, result.Telemetry, dateTimeProvider.UtcNow, ct);
        }
        catch (AiGenerationFailedException ex)
        {
            finalizeError = ex.Message;
        }

        if (finalizeError is not null)
        {
            yield return new RoadmapGenerationErrorEvent(finalizeError);
            yield break;
        }

        yield return new RoadmapGenerationCompleteEvent(roadmap!);
    }
}
