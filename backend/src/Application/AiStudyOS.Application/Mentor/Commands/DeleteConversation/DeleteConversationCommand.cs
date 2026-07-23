using Mediator;

namespace AiStudyOS.Application.Mentor.Commands.DeleteConversation;

public record DeleteConversationCommand(Guid ConversationId) : ICommand<bool>;
