using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Quiz;

public class QuizAnswer : AggregateRoot
{
    public Guid AttemptId { get; private set; }
    public Guid QuestionId { get; private set; }
    public string UserAnswer { get; private set; } = null!;
    public bool IsCorrect { get; private set; }
    public DateTime AnsweredAtUtc { get; private set; }

    private QuizAnswer() { }

    public static QuizAnswer Create(Guid attemptId, Guid questionId, string userAnswer, bool isCorrect, DateTime nowUtc) => new()
    {
        AttemptId = attemptId,
        QuestionId = questionId,
        UserAnswer = userAnswer,
        IsCorrect = isCorrect,
        AnsweredAtUtc = nowUtc,
    };
}
