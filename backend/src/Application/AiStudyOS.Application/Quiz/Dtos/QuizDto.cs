using AiStudyOS.Domain.Quiz;

namespace AiStudyOS.Application.Quiz.Dtos;

public record QuizDto(
    Guid Id,
    string Title,
    string Topic,
    string Difficulty,
    string QuizType,
    int QuestionCount,
    DateTime CreatedAtUtc,
    IReadOnlyList<QuestionDto> Questions)
{
    public static QuizDto FromDomain(Domain.Quiz.Quiz quiz, IReadOnlyList<QuizQuestion> questions) => new(
        quiz.Id,
        quiz.Title,
        quiz.Topic,
        quiz.Difficulty.ToString(),
        quiz.QuizType.ToString(),
        quiz.QuestionCount,
        quiz.CreatedAtUtc,
        questions.OrderBy(q => q.Order).Select(QuestionDto.FromDomain).ToList());
}
