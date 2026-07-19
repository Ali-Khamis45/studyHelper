namespace AiStudyOS.Application.AI.Context;

public record AiContext(IReadOnlyList<ContextFragment> Fragments, int TotalEstimatedTokens);

public interface IContextBuilder
{
    Task<AiContext> BuildAsync(ContextRequest request, IEnumerable<Type> providerTypes, CancellationToken ct);
}
