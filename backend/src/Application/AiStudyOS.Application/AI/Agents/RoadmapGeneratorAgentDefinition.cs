using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Roadmap.Ai;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

public static class RoadmapGeneratorAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.RoadmapGenerator,
        Name: "Roadmap Generator Agent",
        SystemPrompt: new PromptRef(AgentType.RoadmapGenerator, Version: "v1"),
        ContextProviders: [typeof(RoadmapProfileContextProvider), typeof(GoalContextProvider), typeof(TopicMasteryContextProvider)],
        ToolNames: [],
        MemoryAccess: new MemoryAccessPolicy(CanRead: false, CanWrite: false),
        ExpectedOutputSchema: typeof(RoadmapGenerationResult),
        Strategy: ExecutionStrategy.SingleShot,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
