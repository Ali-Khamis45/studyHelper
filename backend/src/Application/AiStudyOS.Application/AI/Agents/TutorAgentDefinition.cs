using AiStudyOS.Application.AI.Context.Providers;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

/// <summary>
/// The default Mentor persona: teaching, general questions, study/career advice, and motivation —
/// the Intent Classifier's fallback whenever no more specific agent applies.
/// </summary>
public static class TutorAgentDefinition
{
    public static AgentDefinition Create() => new(
        Type: AgentType.Tutor,
        Name: "Tutor Agent",
        SystemPrompt: new PromptRef(AgentType.Tutor, Version: "v1"),
        ContextProviders: [typeof(GoalContextProvider), typeof(TaskContextProvider), typeof(TimeOfDayContextProvider), typeof(MemoryContextProvider), typeof(ConversationContextProvider)],
        ToolNames: [],
        MemoryAccess: new MemoryAccessPolicy(CanRead: true, CanWrite: true),
        ExpectedOutputSchema: null,
        Strategy: ExecutionStrategy.StreamingChat,
        RetryPolicy: new RetryPolicy(),
        AllowedProviders: ["ollama"]);
}
