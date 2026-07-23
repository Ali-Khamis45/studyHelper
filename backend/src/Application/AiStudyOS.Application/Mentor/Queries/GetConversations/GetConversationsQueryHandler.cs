using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Mentor.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Mentor.Queries.GetConversations;

public class GetConversationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IOptions<MentorOptions> options)
    : IQueryHandler<GetConversationsQuery, PagedResult<ConversationDto>>
{
    public async ValueTask<PagedResult<ConversationDto>> Handle(GetConversationsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize > 0 ? query.PageSize : options.Value.DefaultPageSize;

        var conversationsQuery = db.Conversations.Where(c => c.UserId == userId);

        if (query.PinnedOnly == true)
            conversationsQuery = conversationsQuery.Where(c => c.IsPinned);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // ToLower().Contains() rather than EF.Functions.ILike: Application must not depend on
            // the Npgsql-specific EF provider package (that lives only in Infrastructure).
            var term = query.Search.Trim().ToLowerInvariant();
            conversationsQuery = conversationsQuery.Where(c => c.Title.ToLower().Contains(term));
        }

        var totalCount = await conversationsQuery.CountAsync(ct);

        var conversations = await conversationsQuery
            .OrderByDescending(c => c.IsPinned)
            .ThenByDescending(c => c.LastMessageAtUtc ?? c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = conversations.Select(ConversationDto.FromDomain).ToList();
        return new PagedResult<ConversationDto>(items, totalCount, page, pageSize);
    }
}
