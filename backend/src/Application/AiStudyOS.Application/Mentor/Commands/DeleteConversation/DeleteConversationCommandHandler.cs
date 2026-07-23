using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Mentor.Commands.DeleteConversation;

public class DeleteConversationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : ICommandHandler<DeleteConversationCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteConversationCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == command.ConversationId && c.UserId == userId, ct);
        if (conversation is null) return false;

        // Messages cascade-delete at the DB level (ConversationMessageConfiguration FK), so
        // removing the conversation is sufficient.
        db.Conversations.Remove(conversation);
        await db.SaveChangesAsync(ct);

        return true;
    }
}
