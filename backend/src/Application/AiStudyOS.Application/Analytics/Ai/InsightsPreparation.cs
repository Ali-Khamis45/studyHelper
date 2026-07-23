using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.Analytics.Ai;

/// <summary>Shared by GetInsightsQueryHandler (auto-generate-if-expired) and RegenerateInsightsCommandHandler — mirrors RecommendationPreparation/QuizPreparation.</summary>
public static class InsightsPreparation
{
    public static async Task<(AgentDefinition Agent, AiContext Context, PromptDefinition Prompt)> PrepareAsync(
        IAgentRegistry agentRegistry, IContextBuilder contextBuilder, IPromptLibrary promptLibrary, Guid userId, CancellationToken ct)
    {
        var agentDefinition = agentRegistry.Resolve(AgentType.Insights);

        var context = await contextBuilder.BuildAsync(
            new ContextRequest(userId, new Dictionary<string, object?>()),
            agentDefinition.ContextProviders,
            ct);

        var prompt = await promptLibrary.GetAsync(AgentType.Insights, agentDefinition.SystemPrompt.Version, ct);

        return (agentDefinition, context, prompt);
    }
}
