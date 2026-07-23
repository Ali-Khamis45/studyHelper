using AiStudyOS.Application.Mentor.Dtos;
using Mediator;

namespace AiStudyOS.Application.Mentor.Commands.SetConversationPinned;

public record SetConversationPinnedCommand(Guid ConversationId, bool IsPinned) : ICommand<ConversationDto>;
