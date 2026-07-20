using AiStudyOS.Application.AI.Kernel;
using Mediator;

namespace AiStudyOS.Application.Diagnostics.Queries.GetAiHealth;

public record GetAiHealthQuery : IQuery<AiHealthResult>;

public class GetAiHealthQueryHandler(IAiKernel aiKernel) : IQueryHandler<GetAiHealthQuery, AiHealthResult>
{
    public async ValueTask<AiHealthResult> Handle(GetAiHealthQuery query, CancellationToken ct) =>
        await aiKernel.CheckHealthAsync(ct);
}
