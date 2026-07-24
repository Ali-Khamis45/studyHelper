using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Roadmap.Commands.CompleteTopic;

public class CompleteTopicCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CompleteTopicCommand, bool>
{
    public async ValueTask<bool> Handle(CompleteTopicCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var roadmap = await db.LearningRoadmaps.FirstOrDefaultAsync(r => r.Id == command.RoadmapId && r.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Roadmap.LearningRoadmap), command.RoadmapId);

        var topic = await db.RoadmapTopics.FirstOrDefaultAsync(t => t.Id == command.TopicId && t.RoadmapId == roadmap.Id, ct);
        if (topic is null) return false;

        var now = dateTimeProvider.UtcNow;
        if (command.Completed) topic.MarkComplete(now);
        else topic.MarkIncomplete(now);

        roadmap.Touch(now);
        await db.SaveChangesAsync(ct);

        return true;
    }
}
