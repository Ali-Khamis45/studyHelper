using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetMentorAnalytics;

public class GetMentorAnalyticsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetMentorAnalyticsQuery, MentorAnalyticsDto>
{
    public async ValueTask<MentorAnalyticsDto> Handle(GetMentorAnalyticsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        return await AnalyticsQueryHelpers.ComputeMentorAnalyticsAsync(db, userId, ct);
    }
}
