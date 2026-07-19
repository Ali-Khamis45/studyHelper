using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Routing;

public record ConversationContext(Guid ConversationId, Guid UserId, IReadOnlyList<string> RecentMessages);

public record IntentResult(AgentType Intent, double Confidence, string? MatchedRule);

public interface IIntentClassifier
{
    Task<IntentResult> ClassifyAsync(string message, ConversationContext context, CancellationToken ct);
}
