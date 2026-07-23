using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Mentor.Dtos;
using Mediator;

namespace AiStudyOS.Application.Mentor.Queries.GetConversations;

public record GetConversationsQuery(string? Search, bool? PinnedOnly, int Page = 1, int PageSize = 0) : IQuery<PagedResult<ConversationDto>>;
