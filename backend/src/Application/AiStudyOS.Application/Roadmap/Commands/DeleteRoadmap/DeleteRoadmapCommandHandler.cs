using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Roadmap.Commands.DeleteRoadmap;

public class DeleteRoadmapCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : ICommandHandler<DeleteRoadmapCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteRoadmapCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var roadmap = await db.LearningRoadmaps.FirstOrDefaultAsync(r => r.Id == command.RoadmapId && r.UserId == userId, ct);
        if (roadmap is null) return false;

        // Topics cascade-delete at the DB level (see RoadmapTopicConfiguration's RoadmapId FK).
        db.LearningRoadmaps.Remove(roadmap);
        await db.SaveChangesAsync(ct);

        return true;
    }
}
