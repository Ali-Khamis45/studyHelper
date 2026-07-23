using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

/// <summary>
/// The conversational counterpart to RecommendationAgentDefinition: discusses planning/goals in
/// free-form chat rather than emitting a structured daily plan. Distinct AgentDefinition, same
/// AgentType.Planner slot in the registry — the Recommendation flow never resolves this type.
/// </summary>
public static class PlannerChatAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.Planner,
        Name: "Planner Chat Agent",
        SystemPrompt: new PromptRef(AgentType.Planner, Version: "v1"),
        ContextProviders: [typeof(GoalContextProvider), typeof(TaskContextProvider), typeof(TimeOfDayContextProvider), typeof(ConversationContextProvider)],
        ToolNames: [],
        MemoryAccess: new MemoryAccessPolicy(CanRead: true, CanWrite: true),
        ExpectedOutputSchema: null,
        Strategy: ExecutionStrategy.StreamingChat,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
