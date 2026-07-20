using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Tools;

public record ToolInvocation(Guid UserId, AgentType AgentType, IReadOnlyDictionary<string, object?> Parameters);

public record ToolResult(bool Success, object? Data, string? Error)
{
    public static ToolResult Ok(object? data) => new(true, data, null);
    public static ToolResult Failed(string error) => new(false, null, error);
}

public interface ITool
{
    string Name { get; }
    string Description { get; }
    IReadOnlyList<AgentType> AllowedAgents { get; }
    Task<ToolResult> ExecuteAsync(ToolInvocation invocation, CancellationToken ct);
}
