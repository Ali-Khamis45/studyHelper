using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Prompts;

public record PromptRef(AgentType AgentType, string? Version = null);

public record PromptDefinition(
    AgentType AgentType,
    string Version,
    string Description,
    IReadOnlyList<string> Variables,
    string? ExpectedJsonSchema,
    IReadOnlyList<string> SupportedModels,
    string Template);

public interface IPromptLibrary
{
    Task<PromptDefinition> GetAsync(AgentType agentType, string? version = null, CancellationToken ct = default);
}
