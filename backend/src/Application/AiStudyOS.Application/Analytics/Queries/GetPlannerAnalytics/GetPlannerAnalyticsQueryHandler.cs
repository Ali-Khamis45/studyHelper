using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetPlannerAnalytics;

public class GetPlannerAnalyticsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetPlannerAnalyticsQuery, PlannerAnalyticsDto>
{
    public async ValueTask<PlannerAnalyticsDto> Handle(GetPlannerAnalyticsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        return await AnalyticsQueryHelpers.ComputePlannerAnalyticsAsync(db, userId, ct);
    }
}
