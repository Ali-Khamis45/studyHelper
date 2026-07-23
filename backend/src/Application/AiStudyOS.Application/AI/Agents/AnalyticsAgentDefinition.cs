using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

public static class AnalyticsAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.Analytics,
        Name: "Analytics Agent",
        SystemPrompt: new PromptRef(AgentType.Analytics, Version: "v1"),
        ContextProviders: [typeof(AnalyticsSnapshotContextProvider), typeof(GoalContextProvider), typeof(ConversationContextProvider)],
        ToolNames: [],
        MemoryAccess: new MemoryAccessPolicy(CanRead: true, CanWrite: false),
        ExpectedOutputSchema: null,
        Strategy: ExecutionStrategy.StreamingChat,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
