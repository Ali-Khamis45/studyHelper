namespace AiStudyOS.Application.AI.Context;

public record ContextRequest(Guid UserId, IReadOnlyDictionary<string, object?> Parameters);

public record ContextFragment(string SectionName, string Content, int EstimatedTokens, int Priority);

public interface IContextProvider
{
    string SectionName { get; }
    Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct);
}
