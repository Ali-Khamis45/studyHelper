using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

/// <summary>
/// Recognizes career/learning-goal statements in Mentor chat ("I want to become a Backend .NET
/// Developer") and nudges the user toward the dedicated Roadmap creation flow, rather than
/// generating a roadmap inline — Mentor's chat surface has no mechanism for an embedded,
/// clickable AI action, so the real generation always happens through RoadmapGenerator via the
/// Roadmap section's own "Create New Learning Journey" flow.
/// </summary>
public static class RoadmapChatAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.RoadmapChat,
        Name: "Roadmap Chat Agent",
        SystemPrompt: new PromptRef(AgentType.RoadmapChat, Version: "v1"),
        ContextProviders: [typeof(GoalContextProvider), typeof(ConversationContextProvider), typeof(TimeOfDayContextProvider)],
        ToolNames: [],
        MemoryAccess: new MemoryAccessPolicy(CanRead: true, CanWrite: true),
        ExpectedOutputSchema: null,
        Strategy: ExecutionStrategy.StreamingChat,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
