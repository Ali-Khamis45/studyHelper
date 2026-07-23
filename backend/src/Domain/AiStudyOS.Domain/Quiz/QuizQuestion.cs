using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Quiz;

/// <summary>Its own table (like ConversationMessage under Conversation), queried by QuizId rather than loaded as a navigation collection.</summary>
public class QuizQuestion : AggregateRoot
{
    public Guid QuizId { get; private set; }
    public int Order { get; private set; }
    public QuestionType Type { get; private set; }
    public string Topic { get; private set; } = null!;
    public Difficulty Difficulty { get; private set; }
    public string Text { get; private set; } = null!;

    /// <summary>JSON array of option strings — only set for MultipleChoice and TrueFalse questions.</summary>
    public string? OptionsJson { get; private set; }
    public string CorrectAnswer { get; private set; } = null!;
    public string Explanation { get; private set; } = null!;

    private QuizQuestion() { }

    public static QuizQuestion Create(
        Guid quizId,
        int order,
        QuestionType type,
        string topic,
        Difficulty difficulty,
        string text,
        string? optionsJson,
        string correctAnswer,
        string explanation) => new()
    {
        QuizId = quizId,
        Order = order,
        Type = type,
        Topic = topic,
        Difficulty = difficulty,
        Text = text,
        OptionsJson = optionsJson,
        CorrectAnswer = correctAnswer,
        Explanation = explanation,
    };
}
