namespace AiStudyOS.Application.Common.Options;

public class QuizOptions
{
    public const string SectionName = "Quiz";

    public int MinQuestionCount { get; init; } = 1;
    public int MaxQuestionCount { get; init; } = 15;
    public int DefaultQuestionCount { get; init; } = 5;

    /// <summary>A topic with MasteryScore below this is considered "weak" for GetWeakTopics and Review-mode generation.</summary>
    public double WeakTopicMasteryThreshold { get; init; } = 0.6;

    public int WeakTopicsDefaultTake { get; init; } = 5;
    public int DefaultPageSize { get; init; } = 20;
}
