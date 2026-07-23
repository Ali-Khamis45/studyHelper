using AiStudyOS.Application.Mentor.Dtos;
using Mediator;

namespace AiStudyOS.Application.Mentor.Commands.SendMessage;

public record SendMessageCommand(Guid ConversationId, string Content) : ICommand<ConversationMessageDto>;
