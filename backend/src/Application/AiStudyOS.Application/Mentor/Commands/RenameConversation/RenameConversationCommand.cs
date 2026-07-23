using AiStudyOS.Application.Mentor.Dtos;
using Mediator;

namespace AiStudyOS.Application.Mentor.Commands.RenameConversation;

public record RenameConversationCommand(Guid ConversationId, string Title) : ICommand<ConversationDto>;
