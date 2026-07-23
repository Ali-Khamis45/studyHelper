using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Quiz.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Quiz.Queries.GetTopicMastery;

public class GetTopicMasteryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetTopicMasteryQuery, IReadOnlyList<TopicMasteryDto>>
{
    public async ValueTask<IReadOnlyList<TopicMasteryDto>> Handle(GetTopicMasteryQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var mastery = await db.TopicMasteries
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.MasteryScore)
            .ToListAsync(ct);

        return mastery.Select(TopicMasteryDto.FromDomain).ToList();
    }
}
