using AiStudyOS.Application.Common.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Diagnostics.Queries.GetAgentMetrics;

public record GetAgentMetricsQuery : IQuery<IReadOnlyList<AgentMetricsDto>>;

/// <summary>
/// Aggregates ai_telemetry_events per AgentType — monitoring only, not consumed by any AI
/// decision-making path (§6).
/// </summary>
public class GetAgentMetricsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetAgentMetricsQuery, IReadOnlyList<AgentMetricsDto>>
{
    public async ValueTask<IReadOnlyList<AgentMetricsDto>> Handle(GetAgentMetricsQuery query, CancellationToken ct)
    {
        return await db.AiTelemetryEvents
            .GroupBy(e => e.AgentType)
            .Select(g => new AgentMetricsDto(
                g.Key,
                g.Count(),
                g.Count(e => e.Success),
                g.Count(e => !e.Success),
                g.Average(e => (double)e.LatencyMs),
                g.Average(e => e.RetryCount > 0 ? 1.0 : 0.0) * 100,
                g.Average(e => (double)(e.PromptTokens + e.CompletionTokens)),
                g.Max(e => e.CreatedAtUtc)))
            .ToListAsync(ct);
    }
}
