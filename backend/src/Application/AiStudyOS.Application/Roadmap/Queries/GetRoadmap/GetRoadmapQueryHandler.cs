using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Roadmap.Ai;
using AiStudyOS.Application.Roadmap.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Roadmap.Queries.GetRoadmap;

public class GetRoadmapQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetRoadmapQuery, RoadmapDto>
{
    public async ValueTask<RoadmapDto> Handle(GetRoadmapQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var roadmap = await db.LearningRoadmaps.FirstOrDefaultAsync(r => r.Id == query.RoadmapId && r.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Roadmap.LearningRoadmap), query.RoadmapId);

        var topics = await db.RoadmapTopics.Where(t => t.RoadmapId == roadmap.Id).ToListAsync(ct);
        var mastery = await RoadmapMasteryLookup.BuildAsync(db, userId, ct);

        return RoadmapProgressCalculator.BuildDto(roadmap, topics, mastery);
    }
}
