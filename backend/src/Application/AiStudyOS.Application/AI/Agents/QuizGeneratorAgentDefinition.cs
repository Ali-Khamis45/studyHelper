using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Quiz.Ai;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

public static class QuizGeneratorAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.Quiz,
        Name: "Quiz Generator Agent",
        SystemPrompt: new PromptRef(AgentType.Quiz, Version: "v1"),
        ContextProviders: [typeof(QuizRequestContextProvider), typeof(GoalContextProvider), typeof(TopicMasteryContextProvider), typeof(QuizHistoryContextProvider)],
        ToolNames: [],
        MemoryAccess: new MemoryAccessPolicy(CanRead: true, CanWrite: false),
        ExpectedOutputSchema: typeof(QuizGenerationResult),
        Strategy: ExecutionStrategy.SingleShot,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
