using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Roadmap.Ai;
using AiStudyOS.Application.Roadmap.Dtos;
using AiStudyOS.Domain.Mentor;
using Mediator;

namespace AiStudyOS.Application.Roadmap.Commands.GenerateRoadmap;

public class GenerateRoadmapCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel) : ICommandHandler<GenerateRoadmapCommand, RoadmapDto>
{
    public async ValueTask<RoadmapDto> Handle(GenerateRoadmapCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var profile = ToProfile(command);

        var (_, context, prompt) = await RoadmapPreparation.PrepareAsync(agentRegistry, contextBuilder, promptLibrary, userId, profile, ct);

        var kernelResult = await aiKernel.ExecuteAsync<RoadmapGenerationResult>(
            new KernelRequest(AgentType.RoadmapGenerator, prompt, context, prompt.ExpectedJsonSchema, UserId: userId), ct);

        if (!kernelResult.Success || kernelResult.Data is null)
            throw new AiGenerationFailedException(string.Join("; ", kernelResult.Errors.DefaultIfEmpty("no response")));

        return await RoadmapFinalizer.FinalizeAsync(db, userId, profile, kernelResult.Data, kernelResult.Telemetry, dateTimeProvider.UtcNow, ct);
    }

    internal static RoadmapProfile ToProfile(GenerateRoadmapCommand command) => new(
        command.CareerGoal, command.CurrentExperience, command.ExistingSkills, command.HoursPerWeek,
        command.LearningStyle, command.TargetCompletionDate, command.PreferredLanguage, command.PreferredResources);
}
