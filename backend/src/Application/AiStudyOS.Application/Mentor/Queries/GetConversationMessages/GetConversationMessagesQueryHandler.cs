using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Mentor.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Mentor.Queries.GetConversationMessages;

/// <summary>
/// Paginates newest-first (page 1 = most recent messages), matching how a chat UI loads: render
/// the tail of the conversation immediately, then "load more" walks backward in time. Each
/// returned page is re-sorted ascending so callers can render it top-to-bottom without re-sorting.
/// </summary>
public class GetConversationMessagesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IOptions<MentorOptions> options)
    : IQueryHandler<GetConversationMessagesQuery, PagedResult<ConversationMessageDto>>
{
    public async ValueTask<PagedResult<ConversationMessageDto>> Handle(GetConversationMessagesQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var conversationExists = await db.Conversations.AnyAsync(c => c.Id == query.ConversationId && c.UserId == userId, ct);
        if (!conversationExists)
            throw new NotFoundException(nameof(Domain.Mentor.Conversation), query.ConversationId);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize > 0 ? query.PageSize : options.Value.DefaultPageSize;

        var messagesQuery = db.ConversationMessages.Where(m => m.ConversationId == query.ConversationId);
        var totalCount = await messagesQuery.CountAsync(ct);

        var messages = await messagesQuery
            .OrderByDescending(m => m.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        messages.Reverse();

        var items = messages.Select(ConversationMessageDto.FromDomain).ToList();
        return new PagedResult<ConversationMessageDto>(items, totalCount, page, pageSize);
    }
}
