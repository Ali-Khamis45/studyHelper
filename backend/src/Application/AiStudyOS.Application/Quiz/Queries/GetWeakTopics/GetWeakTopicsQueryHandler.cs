using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Quiz.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Quiz.Queries.GetWeakTopics;

public class GetWeakTopicsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IOptions<QuizOptions> options)
    : IQueryHandler<GetWeakTopicsQuery, IReadOnlyList<TopicMasteryDto>>
{
    public async ValueTask<IReadOnlyList<TopicMasteryDto>> Handle(GetWeakTopicsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var take = query.Take is > 0 ? query.Take.Value : options.Value.WeakTopicsDefaultTake;

        var mastery = await db.TopicMasteries
            .Where(m => m.UserId == userId && m.MasteryScore < options.Value.WeakTopicMasteryThreshold)
            .OrderBy(m => m.MasteryScore)
            .Take(take)
            .ToListAsync(ct);

        return mastery.Select(TopicMasteryDto.FromDomain).ToList();
    }
}
