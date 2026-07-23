using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using Mediator;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Analytics.Queries.GetMasteryAnalytics;

public class GetMasteryAnalyticsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IOptions<AnalyticsOptions> options)
    : IQueryHandler<GetMasteryAnalyticsQuery, MasteryAnalyticsDto>
{
    public async ValueTask<MasteryAnalyticsDto> Handle(GetMasteryAnalyticsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        return await AnalyticsQueryHelpers.ComputeMasteryAnalyticsAsync(db, userId, options.Value, ct);
    }
}
