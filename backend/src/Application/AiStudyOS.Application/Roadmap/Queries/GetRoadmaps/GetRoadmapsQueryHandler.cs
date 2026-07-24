using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Roadmap.Ai;
using AiStudyOS.Application.Roadmap.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Roadmap.Queries.GetRoadmaps;

public class GetRoadmapsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetRoadmapsQuery, IReadOnlyList<RoadmapSummaryDto>>
{
    public async ValueTask<IReadOnlyList<RoadmapSummaryDto>> Handle(GetRoadmapsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var roadmaps = await db.LearningRoadmaps
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UpdatedAtUtc)
            .ToListAsync(ct);

        if (roadmaps.Count == 0)
            return [];

        var roadmapIds = roadmaps.Select(r => r.Id).ToList();
        var topics = await db.RoadmapTopics.Where(t => roadmapIds.Contains(t.RoadmapId)).ToListAsync(ct);
        var mastery = await RoadmapMasteryLookup.BuildAsync(db, userId, ct);

        return roadmaps
            .Select(r => RoadmapProgressCalculator.BuildSummary(r, topics.Where(t => t.RoadmapId == r.Id).ToList(), mastery))
            .ToList();
    }
}
