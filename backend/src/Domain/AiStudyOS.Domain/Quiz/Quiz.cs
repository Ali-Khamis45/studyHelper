using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Quiz;

public class Quiz : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid? GoalId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Topic { get; private set; } = null!;
    public Difficulty Difficulty { get; private set; }
    public QuizType QuizType { get; private set; }
    public int QuestionCount { get; private set; }
    public string ModelUsed { get; private set; } = null!;
    public string PromptVersion { get; private set; } = null!;
    public string CorrelationId { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private Quiz() { }

    public static Quiz Create(
        Guid userId,
        Guid? goalId,
        string title,
        string topic,
        Difficulty difficulty,
        QuizType quizType,
        int questionCount,
        string modelUsed,
        string promptVersion,
        string correlationId,
        DateTime nowUtc) => new()
    {
        UserId = userId,
        GoalId = goalId,
        Title = title,
        Topic = topic,
        Difficulty = difficulty,
        QuizType = quizType,
        QuestionCount = questionCount,
        ModelUsed = modelUsed,
        PromptVersion = promptVersion,
        CorrelationId = correlationId,
        CreatedAtUtc = nowUtc,
    };
}
