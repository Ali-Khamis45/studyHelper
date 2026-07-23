namespace AiStudyOS.Application.Quiz.Dtos;

/// <summary>The post-grading shape — unlike QuestionDto, this includes the correct answer and explanation, since the attempt is already complete.</summary>
public record AnswerResultDto(
    Guid QuestionId,
    string QuestionText,
    string Topic,
    string UserAnswer,
    string CorrectAnswer,
    bool IsCorrect,
    string Explanation);
