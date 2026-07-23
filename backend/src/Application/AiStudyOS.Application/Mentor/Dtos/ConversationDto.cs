using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.Mentor.Dtos;

public record ConversationDto(
    Guid Id,
    string Title,
    bool IsPinned,
    int MessageCount,
    int TotalPromptTokens,
    int TotalCompletionTokens,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? LastMessageAtUtc)
{
    public static ConversationDto FromDomain(Conversation conversation) => new(
        conversation.Id,
        conversation.Title,
        conversation.IsPinned,
        conversation.MessageCount,
        conversation.TotalPromptTokens,
        conversation.TotalCompletionTokens,
        conversation.CreatedAtUtc,
        conversation.UpdatedAtUtc,
        conversation.LastMessageAtUtc);
}
