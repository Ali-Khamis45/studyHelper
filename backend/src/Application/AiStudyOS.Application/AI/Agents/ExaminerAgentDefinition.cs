using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

/// <summary>
/// Handles quiz-related conversation (practice questions, topic selection) in chat form. The
/// persisted, gradeable Quiz Engine is a separate, not-yet-built milestone (M8) — this agent's
/// prompt is written to stay honest about that boundary rather than imply it can grade or track
/// quiz attempts it has no storage for.
/// </summary>
public static class ExaminerAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.Examiner,
        Name: "Examiner Agent",
        SystemPrompt: new PromptRef(AgentType.Examiner, Version: "v1"),
        ContextProviders: [typeof(GoalContextProvider), typeof(TaskContextProvider), typeof(ConversationContextProvider)],
        ToolNames: [],
        MemoryAccess: new MemoryAccessPolicy(CanRead: true, CanWrite: false),
        ExpectedOutputSchema: null,
        Strategy: ExecutionStrategy.StreamingChat,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
