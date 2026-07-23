namespace AiStudyOS.Application.Quiz.Dtos;

public record QuizSummaryDto(
    Guid Id,
    string Title,
    string Topic,
    string Difficulty,
    string QuizType,
    int QuestionCount,
    DateTime CreatedAtUtc,
    double? LatestAttemptScore,
    DateTime? LatestAttemptAtUtc);
