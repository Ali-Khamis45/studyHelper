using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.Mentor.Dtos;

public record ConversationMessageDto(
    Guid Id,
    Guid ConversationId,
    string Role,
    string Content,
    string? AgentType,
    string? ModelUsed,
    int? PromptTokens,
    int? CompletionTokens,
    DateTime CreatedAtUtc)
{
    public static ConversationMessageDto FromDomain(ConversationMessage message) => new(
        message.Id,
        message.ConversationId,
        message.Role.ToString(),
        message.Content,
        message.AgentType?.ToString(),
        message.ModelUsed,
        message.PromptTokens,
        message.CompletionTokens,
        message.CreatedAtUtc);
}
