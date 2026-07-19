using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Tools;

public interface IToolRegistry
{
    void Register(ITool tool);
    ITool? Resolve(string name);
    IReadOnlyList<ITool> ForAgent(AgentType agentType);
}
