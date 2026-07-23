using System.Text;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Domain.Mentor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.AI.Context.Providers;

/// <summary>
/// Renders the conversation's prior turns (everything before the message currently being answered)
/// as one text fragment. The message actually being answered is NOT included here — it's sent to
/// AiKernel as a real trailing "user" role turn (KernelRequest.UserMessage), not folded into the
/// system prompt: giving Ollama's chat template zero user turns caused it to echo a stray
/// "assistant" role marker into its own output. Priority is deliberately the highest of any Mentor
/// context fragment: if the token budget forces ContextBuilder to drop fragments, goals/tasks/memory
/// go first, never conversation history.
/// </summary>
public class ConversationContextProvider(IApplicationDbContext db, IOptions<MentorOptions> options) : IContextProvider
{
    public string SectionName => "Prior Conversation";

    public async Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var conversationId = (Guid)request.Parameters["conversationId"]!;

        var recent = await db.ConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(options.Value.ConversationHistoryMessages)
            .ToListAsync(ct);

        if (recent.Count == 0)
        {
            const string empty = "This is the first message in the conversation.";
            return new ContextFragment(SectionName, empty, EstimatedTokens: empty.Length / 4, Priority: 200);
        }

        recent.Reverse();

        var sb = new StringBuilder();
        foreach (var message in recent)
        {
            var speaker = message.Role == MessageRole.User ? "User" : "Mentor";
            sb.AppendLine($"{speaker}: {message.Content}");
        }

        var content = sb.ToString().TrimEnd();
        return new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 200);
    }
}
