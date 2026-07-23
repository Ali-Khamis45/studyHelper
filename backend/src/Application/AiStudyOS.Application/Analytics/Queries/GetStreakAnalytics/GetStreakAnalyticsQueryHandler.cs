using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using Mediator;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Analytics.Queries.GetStreakAnalytics;

public class GetStreakAnalyticsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider, IOptions<AnalyticsOptions> options)
    : IQueryHandler<GetStreakAnalyticsQuery, StreakAnalyticsDto>
{
    public async ValueTask<StreakAnalyticsDto> Handle(GetStreakAnalyticsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        return await AnalyticsQueryHelpers.ComputeStreakAnalyticsAsync(db, userId, today, options.Value, ct);
    }
}
