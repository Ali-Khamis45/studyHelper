using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.Planner.Ai;

/// <summary>
/// Shared by GenerateDailyRecommendationCommandHandler (ExecuteAsync path) and RecommendationStreamer
/// (ExecuteStreamAsync path) so both build the exact same context/prompt for the Recommendation
/// agent instead of duplicating that setup.
/// </summary>
public static class RecommendationPreparation
{
    public static async Task<(AgentDefinition Agent, AiContext Context, PromptDefinition Prompt)> PrepareAsync(
        IAgentRegistry agentRegistry, IContextBuilder contextBuilder, IPromptLibrary promptLibrary, Guid userId, CancellationToken ct)
    {
        var agentDefinition = agentRegistry.Resolve(AgentType.Recommendation);

        var context = await contextBuilder.BuildAsync(
            new ContextRequest(userId, new Dictionary<string, object?>()),
            agentDefinition.ContextProviders,
            ct);

        var prompt = await promptLibrary.GetAsync(AgentType.Recommendation, agentDefinition.SystemPrompt.Version, ct);

        return (agentDefinition, context, prompt);
    }
}
