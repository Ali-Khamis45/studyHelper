using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Mentor.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Mentor.Commands.RenameConversation;

public class RenameConversationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RenameConversationCommand, ConversationDto>
{
    public async ValueTask<ConversationDto> Handle(RenameConversationCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == command.ConversationId && c.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Mentor.Conversation), command.ConversationId);

        conversation.Rename(command.Title, dateTimeProvider.UtcNow);
        await db.SaveChangesAsync(ct);

        return ConversationDto.FromDomain(conversation);
    }
}
