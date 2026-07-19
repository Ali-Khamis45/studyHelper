using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

public enum ExecutionStrategy { SingleShot, StreamingChat }

public record RetryPolicy(int MaxAttempts = 2, int BackoffMilliseconds = 500);

public record MemoryAccessPolicy(bool CanRead, bool CanWrite);

/// <summary>
/// Declares an agent's full runtime policy (prompt, context, tools, memory, schema, retry, providers)
/// so a handler executes according to this definition rather than hardcoding it inline.
/// </summary>
public record AgentDefinition(
    AgentType Type,
    string Name,
    PromptRef SystemPrompt,
    IReadOnlyList<Type> ContextProviders,
    IReadOnlyList<string> ToolNames,
    MemoryAccessPolicy MemoryAccess,
    Type? ExpectedOutputSchema,
    ExecutionStrategy Strategy,
    RetryPolicy RetryPolicy,
    IReadOnlyList<string> AllowedProviders);
