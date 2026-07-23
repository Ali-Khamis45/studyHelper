using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Routing;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Mentor;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Mentor.Ai;

/// <summary>
/// The "Supervisor Agent" step of the Mentor pipeline: User -> Supervisor -> Intent Classifier ->
/// Agent Registry -> Context Builder -> Prompt Library -> (caller invokes IAiKernel next). Shared by
/// SendMessageCommandHandler (ExecuteAsync path) and MentorMessageStreamer (ExecuteStreamAsync path)
/// so both route identically instead of duplicating this setup — mirrors RecommendationPreparation.
/// </summary>
public static class MentorOrchestrator
{
    private const int RecentMessagesForClassification = 6;

    public static async Task<(AgentType Intent, AgentDefinition Agent, AiContext Context, PromptDefinition Prompt)> PrepareAsync(
        IApplicationDbContext db,
        IIntentClassifier intentClassifier,
        IAgentRegistry agentRegistry,
        IContextBuilder contextBuilder,
        IPromptLibrary promptLibrary,
        Guid userId,
        Guid conversationId,
        string userMessage,
        CancellationToken ct)
    {
        var recentMessages = await db.ConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(RecentMessagesForClassification)
            .Select(m => m.Content)
            .ToListAsync(ct);
        recentMessages.Reverse();

        var intent = await intentClassifier.ClassifyAsync(userMessage, new ConversationContext(conversationId, userId, recentMessages), ct);
        var agentDefinition = agentRegistry.Resolve(intent.Intent);

        var context = await contextBuilder.BuildAsync(
            new ContextRequest(userId, new Dictionary<string, object?> { ["conversationId"] = conversationId }),
            agentDefinition.ContextProviders,
            ct);

        var prompt = await promptLibrary.GetAsync(agentDefinition.Type, agentDefinition.SystemPrompt.Version, ct);

        return (intent.Intent, agentDefinition, context, prompt);
    }
}
