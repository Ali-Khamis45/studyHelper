using AiStudyOS.Application.Mentor.Dtos;
using Mediator;

namespace AiStudyOS.Application.Mentor.Queries.GetConversation;

public record GetConversationQuery(Guid ConversationId) : IQuery<ConversationDto>;
