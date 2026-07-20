using Microsoft.Extensions.DependencyInjection;

namespace AiStudyOS.Application.AI.Context;

public class ContextBuilder(IServiceProvider serviceProvider) : IContextBuilder
{
    private const int TokenBudget = 4000;

    public async Task<AiContext> BuildAsync(ContextRequest request, IEnumerable<Type> providerTypes, CancellationToken ct)
    {
        var providers = providerTypes.Select(t => (IContextProvider)serviceProvider.GetRequiredService(t)).ToList();

        // Sequential, not Task.WhenAll: providers share the same scoped IApplicationDbContext,
        // and EF Core's DbContext throws if two operations run concurrently on one instance.
        var fragments = new List<ContextFragment>();
        foreach (var provider in providers)
            fragments.Add(await provider.BuildAsync(request, ct));

        // Truncate lowest-priority fragments once the token budget is exhausted, but always keep
        // at least the single highest-priority fragment even if it alone exceeds the budget.
        var selected = new List<ContextFragment>();
        var totalTokens = 0;
        foreach (var fragment in fragments.OrderByDescending(f => f.Priority))
        {
            if (selected.Count > 0 && totalTokens + fragment.EstimatedTokens > TokenBudget)
                continue;

            selected.Add(fragment);
            totalTokens += fragment.EstimatedTokens;
        }

        return new AiContext(selected, totalTokens);
    }
}
