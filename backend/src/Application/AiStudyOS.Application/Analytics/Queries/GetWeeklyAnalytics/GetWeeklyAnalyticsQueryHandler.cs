using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using Mediator;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Analytics.Queries.GetWeeklyAnalytics;

public class GetWeeklyAnalyticsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider, IOptions<AnalyticsOptions> options)
    : IQueryHandler<GetWeeklyAnalyticsQuery, PeriodAnalyticsDto>
{
    public async ValueTask<PeriodAnalyticsDto> Handle(GetWeeklyAnalyticsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var from = today.AddDays(-(options.Value.WeeklyWindowDays - 1));

        var studyTime = await AnalyticsQueryHelpers.ComputeStudyTimeAsync(db, userId, today, ct);
        var tasks = await AnalyticsQueryHelpers.ComputeTaskStatsAsync(db, userId, from, today, ct);
        var dailyActivity = await AnalyticsQueryHelpers.ComputeDailyActivityAsync(db, userId, from, today, ct);

        return new PeriodAnalyticsDto(studyTime, tasks, dailyActivity);
    }
}
