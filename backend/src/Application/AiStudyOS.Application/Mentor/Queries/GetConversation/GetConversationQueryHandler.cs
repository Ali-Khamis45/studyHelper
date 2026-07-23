using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Mentor.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Mentor.Queries.GetConversation;

public class GetConversationQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetConversationQuery, ConversationDto>
{
    public async ValueTask<ConversationDto> Handle(GetConversationQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == query.ConversationId && c.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Mentor.Conversation), query.ConversationId);

        return ConversationDto.FromDomain(conversation);
    }
}
