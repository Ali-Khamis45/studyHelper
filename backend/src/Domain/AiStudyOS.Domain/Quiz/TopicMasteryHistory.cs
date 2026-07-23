using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Quiz;

/// <summary>
/// An append-only snapshot recorded every time TopicMastery.ApplyQuizResult runs — TopicMastery
/// itself only ever holds the current running score, so without this log "mastery evolution over
/// time" (Analytics) would have no real data to plot and would have to be faked. One row per
/// (topic, quiz submission).
/// </summary>
public class TopicMasteryHistory : Entity
{
    public Guid UserId { get; private set; }
    public string Topic { get; private set; } = null!;
    public double MasteryScore { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    private TopicMasteryHistory() { }

    public static TopicMasteryHistory Create(Guid userId, string topic, double masteryScore, DateTime nowUtc) => new()
    {
        UserId = userId,
        Topic = topic,
        MasteryScore = masteryScore,
        RecordedAtUtc = nowUtc,
    };
}
