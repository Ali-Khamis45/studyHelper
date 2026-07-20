using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Planner.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Planner.Queries.GetRecommendationHistory;

/// <summary>
/// Every generated recommendation is persisted and never deleted — even once invalidated — so
/// history is just a read over existing rows, newest first. This also backs "recommendation
/// comparison" on the frontend (pick two rows from this list, diff them client-side).
/// </summary>
public class GetRecommendationHistoryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IOptions<PlannerOptions> plannerOptions)
    : IQueryHandler<GetRecommendationHistoryQuery, IReadOnlyList<PlannerRecommendationDto>>
{
    public async ValueTask<IReadOnlyList<PlannerRecommendationDto>> Handle(GetRecommendationHistoryQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var recommendations = await db.PlannerRecommendations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.GeneratedAt)
            .Take(plannerOptions.Value.RecommendationHistoryLimit)
            .ToListAsync(ct);

        return recommendations.Select(PlannerRecommendationDto.FromDomain).ToList();
    }
}
