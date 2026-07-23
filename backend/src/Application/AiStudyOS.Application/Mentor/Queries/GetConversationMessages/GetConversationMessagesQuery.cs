using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Mentor.Dtos;
using Mediator;

namespace AiStudyOS.Application.Mentor.Queries.GetConversationMessages;

public record GetConversationMessagesQuery(Guid ConversationId, int Page = 1, int PageSize = 0) : IQuery<PagedResult<ConversationMessageDto>>;
