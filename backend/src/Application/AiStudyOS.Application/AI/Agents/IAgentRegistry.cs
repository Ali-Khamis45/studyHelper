using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

public interface IAgentRegistry
{
    void Register(AgentDefinition definition);
    AgentDefinition Resolve(AgentType type);
    IReadOnlyList<AgentDefinition> All { get; }
}
