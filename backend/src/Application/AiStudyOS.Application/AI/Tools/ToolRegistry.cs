using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Tools;

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new();

    public ToolRegistry(IEnumerable<ITool> tools)
    {
        foreach (var tool in tools)
            Register(tool);
    }

    public void Register(ITool tool) => _tools[tool.Name] = tool;

    public ITool? Resolve(string name) => _tools.GetValueOrDefault(name);

    public IReadOnlyList<ITool> ForAgent(AgentType agentType) =>
        _tools.Values.Where(t => t.AllowedAgents.Contains(agentType)).ToList();
}
