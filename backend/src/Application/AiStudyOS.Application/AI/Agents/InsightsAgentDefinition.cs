using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Analytics.Ai;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

public static class InsightsAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.Insights,
        Name: "Insights Agent",
        SystemPrompt: new PromptRef(AgentType.Insights, Version: "v1"),
        ContextProviders: [typeof(GoalContextProvider), typeof(TaskContextProvider), typeof(AnalyticsSnapshotContextProvider), typeof(TopicMasteryContextProvider), typeof(QuizHistoryContextProvider)],
        ToolNames: [],
        MemoryAccess: new MemoryAccessPolicy(CanRead: true, CanWrite: false),
        ExpectedOutputSchema: typeof(InsightsResult),
        Strategy: ExecutionStrategy.SingleShot,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
