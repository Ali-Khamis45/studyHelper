using Microsoft.Extensions.Logging;

namespace AiStudyOS.Application.AI.Tools;

public class ToolExecutor(IToolRegistry registry, ILogger<ToolExecutor> logger) : IToolExecutor
{
    public async Task<ToolResult> ExecuteAsync(string toolName, ToolInvocation invocation, CancellationToken ct)
    {
        var tool = registry.Resolve(toolName);
        if (tool is null)
            return ToolResult.Failed($"Unknown tool '{toolName}'.");

        if (!tool.AllowedAgents.Contains(invocation.AgentType))
            return ToolResult.Failed($"Tool '{toolName}' is not allowed for agent '{invocation.AgentType}'.");

        try
        {
            var result = await tool.ExecuteAsync(invocation, ct);
            logger.LogInformation(
                "Tool {ToolName} executed for user {UserId} by {AgentType}: {Success}",
                toolName, invocation.UserId, invocation.AgentType, result.Success);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tool {ToolName} threw for user {UserId}", toolName, invocation.UserId);
            return ToolResult.Failed($"Tool '{toolName}' failed: {ex.Message}");
        }
    }
}
