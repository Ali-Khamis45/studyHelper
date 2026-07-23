namespace AiStudyOS.Application.Quiz.Ai;

public record GeneratedQuestionResult(
    string Type,
    string Topic,
    string Difficulty,
    string Text,
    IReadOnlyList<string>? Options,
    string CorrectAnswer,
    string Explanation);

public record QuizGenerationResult(string Title, IReadOnlyList<GeneratedQuestionResult> Questions);
