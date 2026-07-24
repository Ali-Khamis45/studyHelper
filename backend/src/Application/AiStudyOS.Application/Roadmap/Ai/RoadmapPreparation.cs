using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.Roadmap.Ai;

public record RoadmapProfile(
    string CareerGoal,
    string? CurrentExperience,
    string? ExistingSkills,
    int? HoursPerWeek,
    string? LearningStyle,
    DateOnly? TargetCompletionDate,
    string? PreferredLanguage,
    string? PreferredResources);

/// <summary>Shared by GenerateRoadmapCommandHandler (ExecuteAsync path) and RoadmapGenerationStreamer (ExecuteStreamAsync path), mirroring QuizPreparation/RecommendationPreparation.</summary>
public static class RoadmapPreparation
{
    public static async Task<(AgentDefinition Agent, AiContext Context, PromptDefinition Prompt)> PrepareAsync(
        IAgentRegistry agentRegistry,
        IContextBuilder contextBuilder,
        IPromptLibrary promptLibrary,
        Guid userId,
        RoadmapProfile profile,
        CancellationToken ct)
    {
        var agentDefinition = agentRegistry.Resolve(AgentType.RoadmapGenerator);

        var context = await contextBuilder.BuildAsync(
            new ContextRequest(userId, new Dictionary<string, object?>
            {
                ["careerGoal"] = profile.CareerGoal,
                ["currentExperience"] = profile.CurrentExperience,
                ["existingSkills"] = profile.ExistingSkills,
                ["hoursPerWeek"] = profile.HoursPerWeek,
                ["learningStyle"] = profile.LearningStyle,
                ["targetCompletionDate"] = profile.TargetCompletionDate?.ToString("yyyy-MM-dd"),
                ["preferredLanguage"] = profile.PreferredLanguage,
                ["preferredResources"] = profile.PreferredResources,
            }),
            agentDefinition.ContextProviders,
            ct);

        var prompt = await promptLibrary.GetAsync(AgentType.RoadmapGenerator, agentDefinition.SystemPrompt.Version, ct);

        return (agentDefinition, context, prompt);
    }
}
