using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Analytics.Ai;
using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Mentor;
using Mediator;

namespace AiStudyOS.Application.Analytics.Commands.RegenerateInsights;

public class RegenerateInsightsCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel) : ICommandHandler<RegenerateInsightsCommand, InsightsDto>
{
    public async ValueTask<InsightsDto> Handle(RegenerateInsightsCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var (_, context, prompt) = await InsightsPreparation.PrepareAsync(agentRegistry, contextBuilder, promptLibrary, userId, ct);

        var kernelResult = await aiKernel.ExecuteAsync<InsightsResult>(
            new KernelRequest(AgentType.Insights, prompt, context, prompt.ExpectedJsonSchema, UserId: userId), ct);

        if (!kernelResult.Success || kernelResult.Data is null)
            throw new AiGenerationFailedException(string.Join("; ", kernelResult.Errors.DefaultIfEmpty("no response")));

        return await InsightsFinalizer.FinalizeAsync(db, userId, kernelResult.Data, kernelResult.Telemetry, dateTimeProvider.UtcNow, ct);
    }
}
