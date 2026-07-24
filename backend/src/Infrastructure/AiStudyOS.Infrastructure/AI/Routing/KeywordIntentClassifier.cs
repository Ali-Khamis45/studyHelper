using AiStudyOS.Application.AI.Routing;
using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Infrastructure.AI.Routing;

/// <summary>
/// Rule-based, not LLM-based: routing runs on every message before any provider call, so it must
/// stay fast and free. Rules are data (a table of AgentType -> keyword set), not branching code —
/// AgentRegistry, not this class, decides what a matched AgentType actually does (prompt, context,
/// tools). Adding/removing an intent means editing the Rules table, never adding an if/switch.
/// </summary>
public class KeywordIntentClassifier : IIntentClassifier
{
    private sealed record IntentRule(AgentType Intent, string[] Keywords);

    private static readonly IntentRule[] Rules =
    [
        new(AgentType.Planner, ["plan", "schedule", "today", "task", "goal", "deadline", "reschedule", "organize", "priorit", "due date"]),
        new(AgentType.Analytics, ["progress", "streak", "analytics", "stats", "statistic", "performance", "completion rate", "how am i doing", "track my"]),
        new(AgentType.Examiner, ["quiz", "test me", "practice question", "flashcard", "exam me", "ask me questions", "practice test"]),
        new(AgentType.Tutor, ["explain", "teach", "understand", "help me learn", "career", "motivat", "advice", "confused", "study tip", "how do i"]),
        new(AgentType.RoadmapChat, ["want to become", "learning roadmap", "learning journey", "learning path", "career path", "roadmap for", "how do i become"]),
    ];

    private const AgentType FallbackIntent = AgentType.Tutor;

    public Task<IntentResult> ClassifyAsync(string message, ConversationContext context, CancellationToken ct)
    {
        var lower = message.ToLowerInvariant();

        var scored = Rules
            .Select(rule => (rule.Intent, Score: rule.Keywords.Count(lower.Contains)))
            .Where(r => r.Score > 0)
            .OrderByDescending(r => r.Score)
            .ToList();

        if (scored.Count == 0)
            return Task.FromResult(new IntentResult(FallbackIntent, Confidence: 0.3, MatchedRule: "fallback"));

        var best = scored[0];
        var confidence = Math.Min(1.0, 0.5 + best.Score * 0.15);

        return Task.FromResult(new IntentResult(best.Intent, confidence, MatchedRule: best.Intent.ToString()));
    }
}
