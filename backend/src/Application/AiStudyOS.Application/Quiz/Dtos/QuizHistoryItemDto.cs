namespace AiStudyOS.Application.Quiz.Dtos;

public record QuizHistoryItemDto(
    Guid AttemptId,
    Guid QuizId,
    string QuizTitle,
    string Topic,
    double? Score,
    int CorrectCount,
    int TotalCount,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);
