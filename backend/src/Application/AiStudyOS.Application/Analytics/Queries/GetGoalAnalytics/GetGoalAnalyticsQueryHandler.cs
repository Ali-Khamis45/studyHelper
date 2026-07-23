using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetGoalAnalytics;

public class GetGoalAnalyticsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetGoalAnalyticsQuery, GoalAnalyticsDto>
{
    public async ValueTask<GoalAnalyticsDto> Handle(GetGoalAnalyticsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        return await AnalyticsQueryHelpers.ComputeGoalAnalyticsAsync(db, userId, ct);
    }
}
