using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Quiz;

public class QuizAttempt : AggregateRoot
{
    public Guid QuizId { get; private set; }
    public Guid UserId { get; private set; }
    public AttemptStatus Status { get; private set; }
    public double? Score { get; private set; }
    public int CorrectCount { get; private set; }
    public int TotalCount { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private QuizAttempt() { }

    public static QuizAttempt Create(Guid quizId, Guid userId, DateTime nowUtc) => new()
    {
        QuizId = quizId,
        UserId = userId,
        Status = AttemptStatus.InProgress,
        Score = null,
        CorrectCount = 0,
        TotalCount = 0,
        StartedAtUtc = nowUtc,
        CompletedAtUtc = null,
    };

    public void Complete(int correctCount, int totalCount, DateTime nowUtc)
    {
        CorrectCount = correctCount;
        TotalCount = totalCount;
        Score = totalCount == 0 ? 0 : Math.Round(100.0 * correctCount / totalCount, 1);
        Status = AttemptStatus.Completed;
        CompletedAtUtc = nowUtc;
    }
}
