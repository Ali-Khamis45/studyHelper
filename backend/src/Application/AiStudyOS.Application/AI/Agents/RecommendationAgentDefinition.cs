using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Planner.Ai;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

public static class RecommendationAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.Recommendation,
        Name: "Recommendation Agent",
        SystemPrompt: new PromptRef(AgentType.Recommendation, Version: "v1"),
        ContextProviders: [typeof(GoalContextProvider), typeof(TaskContextProvider), typeof(TimeOfDayContextProvider)],
        ToolNames: ["planner"],
        MemoryAccess: new MemoryAccessPolicy(CanRead: false, CanWrite: false),
        ExpectedOutputSchema: typeof(RecommendationResult),
        Strategy: ExecutionStrategy.SingleShot,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
