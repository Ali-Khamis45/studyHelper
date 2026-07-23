using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Memory;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Mentor.Dtos;
using AiStudyOS.Domain.Mentor;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Mentor.Ai;

/// <summary>
/// The persistence half of the Mentor pipeline (-> Provider -> Telemetry -> Persistence). Split
/// into two phases — user message saved immediately, assistant message saved only after a
/// successful AI response — so a failed or cancelled generation still leaves the conversation in a
/// consistent state: the user's message is never lost, and a partial/failed reply is never saved.
/// </summary>
public class MentorConversationStore(IApplicationDbContext db, IMemoryStore memoryStore, IOptions<MentorOptions> options)
{
    public async Task<Domain.Mentor.Conversation> AppendUserMessageAsync(Domain.Mentor.Conversation conversation, Guid userId, string content, DateTime nowUtc, CancellationToken ct)
    {
        var message = ConversationMessage.CreateUserMessage(conversation.Id, userId, content, nowUtc);
        db.ConversationMessages.Add(message);

        if (conversation.HasDefaultTitle && conversation.MessageCount == 0)
            conversation.Rename(DeriveTitle(content), nowUtc);

        await db.SaveChangesAsync(ct);
        return conversation;
    }

    public async Task<ConversationMessageDto> AppendAssistantMessageAsync(
        Domain.Mentor.Conversation conversation,
        Guid userId,
        string content,
        AgentType agentType,
        AiTelemetryRecord telemetry,
        AgentDefinition agentDefinition,
        string userContent,
        DateTime nowUtc,
        CancellationToken ct)
    {
        var message = ConversationMessage.CreateAssistantMessage(
            conversation.Id, userId, content, agentType, telemetry.Model, telemetry.PromptTokens, telemetry.CompletionTokens, telemetry.CorrelationId, nowUtc);

        db.ConversationMessages.Add(message);
        conversation.RecordExchange(telemetry.PromptTokens, telemetry.CompletionTokens, nowUtc);

        await db.SaveChangesAsync(ct);

        if (agentDefinition.MemoryAccess.CanWrite)
            await WriteMemoryIfAppropriateAsync(userId, conversation.Id, userContent, content, nowUtc, ct);

        return ConversationMessageDto.FromDomain(message);
    }

    /// <summary>
    /// Deterministic, not model-driven: a short exchange (greetings, acknowledgements) is skipped
    /// as not durably useful; anything longer is stored verbatim as real conversation content, never
    /// fabricated. Salience nudges up when the exchange mentions concrete, revisitable study
    /// context (goals, deadlines, exams) so ContextBuilder's later token-budget trimming keeps the
    /// most useful memories first.
    /// </summary>
    private async Task WriteMemoryIfAppropriateAsync(Guid userId, Guid conversationId, string userContent, string assistantContent, DateTime nowUtc, CancellationToken ct)
    {
        if (userContent.Length < options.Value.MemoryWriteMinContentLength)
            return;

        var salience = ComputeSalience(userContent);
        var summary = $"User asked: {Truncate(userContent, 300)} | Mentor advised: {Truncate(assistantContent, 300)}";

        await memoryStore.WriteAsync(new MemoryRecordDto(userId, MemoryType.Learning, Topic: null, summary, salience, "Conversation", nowUtc, SourceId: conversationId), ct);
    }

    private static readonly string[] SalienceKeywords = ["goal", "deadline", "exam", "test", "struggl", "confus", "career", "grade"];

    private static double ComputeSalience(string content)
    {
        var lower = content.ToLowerInvariant();
        var hits = SalienceKeywords.Count(lower.Contains);
        return Math.Clamp(0.4 + hits * 0.15, 0, 1);
    }

    private static string DeriveTitle(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.Length <= 60) return trimmed;

        var cut = trimmed[..60];
        var lastSpace = cut.LastIndexOf(' ');
        return (lastSpace > 20 ? cut[..lastSpace] : cut).TrimEnd() + "...";
    }

    private static string Truncate(string content, int maxLength) =>
        content.Length <= maxLength ? content : content[..maxLength].TrimEnd() + "...";
}
