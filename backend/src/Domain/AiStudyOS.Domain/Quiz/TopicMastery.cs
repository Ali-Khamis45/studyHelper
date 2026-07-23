using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Quiz;

/// <summary>
/// One row per (UserId, Topic). MasteryScore is a 0-1 estimate of how well the student knows a
/// topic, updated after every quiz that includes questions on it — never computed from scratch,
/// so it reflects a running history rather than only the most recent attempt.
/// </summary>
public class TopicMastery : AggregateRoot
{
    public const double DefaultSmoothingFactor = 0.3;

    public Guid UserId { get; private set; }
    public string Topic { get; private set; } = null!;
    public double MasteryScore { get; private set; }
    public int AttemptsCount { get; private set; }
    public DateTime LastUpdatedUtc { get; private set; }

    private TopicMastery() { }

    public static TopicMastery Create(Guid userId, string topic, double initialScore, DateTime nowUtc) => new()
    {
        UserId = userId,
        Topic = topic,
        MasteryScore = Math.Clamp(initialScore, 0, 1),
        AttemptsCount = 1,
        LastUpdatedUtc = nowUtc,
    };

    /// <summary>
    /// Exponentially-weighted moving average: MasteryScore = MasteryScore * (1 - alpha) + topicScore * alpha.
    /// alpha (default 0.3) is how much weight the newest attempt carries — high enough that mastery
    /// responds to genuine improvement or regression within a handful of quizzes, low enough that one
    /// lucky or unlucky attempt doesn't swing the estimate. topicScore is this attempt's own
    /// difficulty-weighted correctness for the topic (see QuizGrader.ComputeTopicScore) — harder
    /// questions answered correctly count for more than easy ones, in both this update and the score
    /// it's derived from.
    /// </summary>
    public void ApplyQuizResult(double topicScore, DateTime nowUtc, double alpha = DefaultSmoothingFactor)
    {
        MasteryScore = Math.Clamp(MasteryScore * (1 - alpha) + topicScore * alpha, 0, 1);
        AttemptsCount++;
        LastUpdatedUtc = nowUtc;
    }
}
