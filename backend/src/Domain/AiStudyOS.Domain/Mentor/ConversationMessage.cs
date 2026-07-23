using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Mentor;

/// <summary>
/// Deliberately its own aggregate/table (like DailyTask under Goal) rather than a collection
/// navigation loaded off Conversation — conversations can accumulate thousands of messages, and
/// callers always page through them explicitly instead of loading the whole history at once.
/// </summary>
public class ConversationMessage : AggregateRoot
{
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = null!;

    /// <summary>Which specialist agent produced this reply. Null for user messages.</summary>
    public AgentType? AgentType { get; private set; }
    public string? ModelUsed { get; private set; }
    public int? PromptTokens { get; private set; }
    public int? CompletionTokens { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ConversationMessage() { }

    public static ConversationMessage CreateUserMessage(Guid conversationId, Guid userId, string content, DateTime nowUtc) => new()
    {
        ConversationId = conversationId,
        UserId = userId,
        Role = MessageRole.User,
        Content = content,
        AgentType = null,
        ModelUsed = null,
        PromptTokens = null,
        CompletionTokens = null,
        CorrelationId = null,
        CreatedAtUtc = nowUtc,
    };

    public static ConversationMessage CreateAssistantMessage(
        Guid conversationId,
        Guid userId,
        string content,
        AgentType agentType,
        string modelUsed,
        int promptTokens,
        int completionTokens,
        string correlationId,
        DateTime nowUtc) => new()
    {
        ConversationId = conversationId,
        UserId = userId,
        Role = MessageRole.Assistant,
        Content = content,
        AgentType = agentType,
        ModelUsed = modelUsed,
        PromptTokens = promptTokens,
        CompletionTokens = completionTokens,
        CorrelationId = correlationId,
        CreatedAtUtc = nowUtc,
    };
}
