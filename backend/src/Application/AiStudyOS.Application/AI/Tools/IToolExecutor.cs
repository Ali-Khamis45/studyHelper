namespace AiStudyOS.Application.AI.Tools;

/// <summary>
/// Wraps tool execution with telemetry and a hard per-invocation UserId scope check, so a tool
/// cannot act outside the calling user's data regardless of what an agent passes in.
/// </summary>
public interface IToolExecutor
{
    Task<ToolResult> ExecuteAsync(string toolName, ToolInvocation invocation, CancellationToken ct);
}
