using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetQuizAnalytics;

public class GetQuizAnalyticsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetQuizAnalyticsQuery, QuizAnalyticsDto>
{
    public async ValueTask<QuizAnalyticsDto> Handle(GetQuizAnalyticsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        return await AnalyticsQueryHelpers.ComputeQuizAnalyticsAsync(db, userId, ct);
    }
}
