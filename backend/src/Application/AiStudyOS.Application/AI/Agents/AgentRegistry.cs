using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Agents;

public class AgentRegistry : IAgentRegistry
{
    private readonly Dictionary<AgentType, AgentDefinition> _definitions = [];

    public void Register(AgentDefinition definition) => _definitions[definition.Type] = definition;

    public AgentDefinition Resolve(AgentType type) =>
        _definitions.TryGetValue(type, out var definition)
            ? definition
            : throw new InvalidOperationException($"No AgentDefinition registered for '{type}'. Registered: [{string.Join(", ", _definitions.Keys)}].");

    public IReadOnlyList<AgentDefinition> All => _definitions.Values.ToList();
}
