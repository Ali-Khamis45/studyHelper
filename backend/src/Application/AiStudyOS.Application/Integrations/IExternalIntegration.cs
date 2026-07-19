namespace AiStudyOS.Application.Integrations;

// Interface-only in Phase 1. Shaped like IToolRegistry/IAiChatClientResolver on purpose, so the
// codebase has one consistent "pluggable provider" idiom across AI providers, tools, and integrations.

public record IntegrationCapabilities(IReadOnlyList<string> SupportedActions);

public record IntegrationRequest(string Action, IReadOnlyDictionary<string, object?> Parameters);

public record IntegrationResult(bool Success, object? Data, string? Error);

public interface IExternalIntegration
{
    string Key { get; }
    IntegrationCapabilities Capabilities { get; }
    Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct);
    Task<bool> IsConnectedAsync(Guid userId, CancellationToken ct);
}
