using AiStudyOS.Application.Mentor.Dtos;
using Mediator;

namespace AiStudyOS.Application.Mentor.Commands.CreateConversation;

public record CreateConversationCommand(string? Title) : ICommand<ConversationDto>;
