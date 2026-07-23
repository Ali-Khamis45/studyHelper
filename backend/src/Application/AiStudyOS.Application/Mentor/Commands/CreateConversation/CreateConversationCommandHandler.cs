using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Mentor.Dtos;
using AiStudyOS.Domain.Mentor;
using Mediator;

namespace AiStudyOS.Application.Mentor.Commands.CreateConversation;

public class CreateConversationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateConversationCommand, ConversationDto>
{
    public async ValueTask<ConversationDto> Handle(CreateConversationCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var conversation = Conversation.Create(userId, command.Title, dateTimeProvider.UtcNow);

        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(ct);

        return ConversationDto.FromDomain(conversation);
    }
}
