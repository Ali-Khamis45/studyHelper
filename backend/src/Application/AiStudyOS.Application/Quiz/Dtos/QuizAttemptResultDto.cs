namespace AiStudyOS.Application.Quiz.Dtos;

public record QuizAttemptResultDto(
    Guid AttemptId,
    Guid QuizId,
    string QuizTitle,
    double Score,
    int CorrectCount,
    int TotalCount,
    DateTime CompletedAtUtc,
    IReadOnlyList<AnswerResultDto> Answers,
    IReadOnlyList<string> WeakTopics,
    IReadOnlyList<string> RecommendedTopics,
    double EstimatedMasteryDelta,
    double Confidence);
