using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Tools;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner.Ai;
using AiStudyOS.Application.Planner.Dtos;
using AiStudyOS.Domain.Mentor;
using Mediator;

namespace AiStudyOS.Application.Planner.Commands.GenerateDailyRecommendation;

public class GenerateDailyRecommendationCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel,
    IToolExecutor toolExecutor) : ICommandHandler<GenerateDailyRecommendationCommand, TodayPlanDto>
{
    public async ValueTask<TodayPlanDto> Handle(GenerateDailyRecommendationCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        var (_, context, prompt) = await RecommendationPreparation.PrepareAsync(agentRegistry, contextBuilder, promptLibrary, userId, ct);

        var kernelResult = await aiKernel.ExecuteAsync<RecommendationResult>(
            new KernelRequest(AgentType.Recommendation, prompt, context, prompt.ExpectedJsonSchema, UserId: userId),
            ct);

        if (!kernelResult.Success || kernelResult.Data is null)
            throw new AiGenerationFailedException(string.Join("; ", kernelResult.Errors.DefaultIfEmpty("no response")));

        return await RecommendationFinalizer.FinalizeAsync(
            db, toolExecutor, userId, today, kernelResult.Data, kernelResult.Telemetry, kernelResult.RawContent, dateTimeProvider.UtcNow, ct);
    }
}
